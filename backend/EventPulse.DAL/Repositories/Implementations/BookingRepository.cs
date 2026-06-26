using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.DTOs.Event;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Context;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Enums;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.DAL.Repositories.Implementations;

public class BookingRepository(EventPulseDbContext context) : IBookingRepository
{
    private readonly EventPulseDbContext _context = context;

    public async Task<Event?> GetEventByIdAsync(int eventId)
    {
        return await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId);
    }

    public async Task<Booking> CreatePendingBookingAsync(
        int userId, int eventId, string uniqueCode, int quantity,
        decimal pricePerTicket, decimal totalAmount, string paymentIntentId)
    {
        Booking booking = new Booking
        {
            UserId = userId,
            EventId = eventId,
            UniqueCode = uniqueCode,
            Quantity = quantity,
            PricePerTicket = pricePerTicket,
            TotalAmount = totalAmount,
            PaymentStatus = PaymentStatus.Pending,
            PaymentRef = paymentIntentId,
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return booking;
    }

    public async Task<(Booking Booking, int RemainingSeats)> ConfirmPaymentAsync(string paymentIntentId)
    {
        Booking? existing = await _context.Bookings
            .FirstOrDefaultAsync(b => b.PaymentRef == paymentIntentId);

        if (existing == null)
            throw new InvalidOperationException("Booking not found for this payment.");

        // Duplicate prevention — already paid, return current state
        if (existing.PaymentStatus == PaymentStatus.Paid)
        {
            int currentSeats = await _context.Events
                .Where(e => e.Id == existing.EventId)
                .Select(e => e.TotalSeats)
                .FirstOrDefaultAsync();

            return (existing, currentSeats);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            Event? tracked = _context.ChangeTracker.Entries<Event>()
                .Select(e => e.Entity)
                .FirstOrDefault(e => e.Id == existing.EventId);
            if (tracked != null)
                _context.Entry(tracked).State = EntityState.Detached;

            await _context.Database.ExecuteSqlRawAsync(
                "SELECT 1 FROM events WHERE id = {0} AND NOT is_deleted FOR UPDATE", existing.EventId);

            Event eventEntity = await _context.Events.FindAsync(existing.EventId)
                ?? throw new InvalidOperationException("Event not found.");

            if (existing.Quantity > eventEntity.TotalSeats)
                throw new InvalidOperationException($"Only {eventEntity.TotalSeats} seats available.");

            eventEntity.TotalSeats -= existing.Quantity;
            existing.PaymentStatus = PaymentStatus.Paid;

            // Generate one unique GUID ticket code per seat
            for (int i = 0; i < existing.Quantity; i++)
            {
                existing.Tickets.Add(new Ticket
                {
                    TicketCode = Guid.NewGuid().ToString("N").ToUpper(),
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (existing, eventEntity.TotalSeats);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task MarkPaymentFailedAsync(string paymentIntentId)
    {
        Booking? booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.PaymentRef == paymentIntentId);

        if (booking == null || booking.PaymentStatus != PaymentStatus.Pending)
            return;

        booking.PaymentStatus = PaymentStatus.Failed;
        await _context.SaveChangesAsync();
    }

    public async Task<BookingResponse?> GetByPaymentIntentAsync(string paymentIntentId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.PaymentRef == paymentIntentId)
            .Select(b => new BookingResponse
            {
                Id = b.Id,
                UserId = b.UserId,
                EventId = b.EventId,
                EventTitle = b.Event!.Title,
                UniqueCode = b.UniqueCode,
                Quantity = b.Quantity,
                PricePerTicket = b.PricePerTicket,
                TotalAmount = b.TotalAmount,
                PaymentStatus = b.PaymentStatus.ToString(),
                PaymentRef = b.PaymentRef,
                Tickets = b.Tickets.Select(t => new TicketDto
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    QrCodeUrl = t.QrCodePath != null ? $"{t.QrCodePath}" : null,
                    PdfUrl = t.PdfPath != null ? $"{t.PdfPath}" : null,
                    IsUsed = t.IsUsed,
                    UsedAt = t.UsedAt,
                }).ToList(),
                TicketCodes = b.Tickets.Select(t => t.TicketCode).ToList(),
                RemainingSeats = b.Event!.TotalSeats,
                CreatedAt = b.CreatedAt,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Booking?> GetBookingByTicketIdAsync(int ticketId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Tickets.Any(t => t.Id == ticketId))
            .Select(b => new Booking
            {
                Id = b.Id,
                UserId = b.UserId,
                EventId = b.EventId,
                Tickets = b.Tickets.Select(t => new Ticket
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    QrCodePath = t.QrCodePath,
                    PdfPath = t.PdfPath,
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<TicketCheckInProjection?> GetTicketByCodeAsync(string ticketCode)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(t => t.TicketCode == ticketCode)
            .Select(t => new TicketCheckInProjection
            {
                TicketId = t.Id,
                TicketCode = t.TicketCode,
                IsUsed = t.IsUsed,
                UsedAt = t.UsedAt,
                BookingId = t.Booking!.Id,
                EventId = t.Booking.Event!.Id,
                EventTitle = t.Booking.Event.Title,
                EventOrganizerId = t.Booking.Event.OrganizerId,
                AttendeeName = t.Booking.User != null
                    ? (t.Booking.User.Name ?? t.Booking.User.Email)
                    : null,
            })
            .FirstOrDefaultAsync();
    }

    public async Task MarkTicketAsUsedAsync(int ticketId)
    {
        Ticket? ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket != null)
        {
            ticket.IsUsed = true;
            ticket.UsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Booking?> GetBookingWithDetailsAsync(int bookingId)
    {
        return await _context.Bookings
            .Where(b => b.Id == bookingId)
            .Include(b => b.Event)!.ThenInclude(e => e!.Venue)
            .Include(b => b.User)
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateTicketPathsAsync(ICollection<Ticket> tickets)
    {
        foreach (Ticket ticket in tickets)
        {
            Ticket? tracked = _context.ChangeTracker.Entries<Ticket>()
                .Select(e => e.Entity)
                .FirstOrDefault(e => e.Id == ticket.Id);

            if (tracked != null)
            {
                tracked.QrCodePath = ticket.QrCodePath;
                tracked.PdfPath = ticket.PdfPath;
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task<List<MyTicketResponse>> GetUserBookingsAsync(int userId, int bookingId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.PaymentStatus == PaymentStatus.Paid && b.Id == bookingId)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new MyTicketResponse
            {
                BookingId = b.Id,
                BookingCode = b.UniqueCode,
                EventTitle = b.Event!.Title,
                EventDate = b.Event.EventDate,
                VenueName = b.Event.Venue != null ? b.Event.Venue.Name : null,
                VenueCity = b.Event != null && b.Event.Venue != null && b.Event.Venue.City != null ? b.Event.Venue.City.Name : null,
                Quantity = b.Quantity,
                TotalAmount = b.TotalAmount,
                CreatedAt = b.CreatedAt,
                Tickets = b.Tickets.Select(t => new TicketDto
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    QrCodeUrl = t.QrCodePath != null ? $"{t.QrCodePath}" : null,
                    PdfUrl = t.PdfPath != null ? $"{t.PdfPath}" : null,
                    IsUsed = t.IsUsed,
                    UsedAt = t.UsedAt,
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<List<BookingProjection>> GetAllBookingsAsync()
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.PaymentStatus == PaymentStatus.Paid)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new BookingProjection
            {
                Id = b.Id,
                UserId = b.UserId,
                EventId = b.EventId,
                Quantity = b.Quantity,
                TotalAmount = b.TotalAmount,
                PaymentStatus = b.PaymentStatus,
                CreatedAt = b.CreatedAt,
                IsDeleted = b.IsDeleted,
            })
            .ToListAsync();
    }

    public async Task<PagedResult<AdminBookingResponse>> GetPagedBookingsAsync(PageRequest pageRequest)
    {
        IQueryable<AdminBookingResponse> query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.PaymentStatus == PaymentStatus.Paid)
            .Select(b => new AdminBookingResponse
            {
                Id = b.Id,
                UserId = b.UserId,
                CustomerName = b.User.Name,
                CustomerEmail = b.User.Email,
                EventId = b.EventId,
                EventTitle = b.Event.Title,
                VenueName = b.Event.Venue != null ? b.Event.Venue.Name : null,
                Quantity = b.Quantity,
                TotalAmount = b.TotalAmount,
                PaymentStatus = b.PaymentStatus.ToString(),
                UniqueCode = b.UniqueCode,
                CreatedAt = b.CreatedAt,
            });

        int totalCount = await query.CountAsync();

        List<AdminBookingResponse> items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageRequest.PageNumber - 1) * pageRequest.PageSize)
            .Take(pageRequest.PageSize)
            .ToListAsync();

        return new PagedResult<AdminBookingResponse>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<MyTicketResponse>> GetPagedUserBookingsAsync(int userId, PageRequest pageRequest)
    {
        IQueryable<MyTicketResponse> query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.PaymentStatus == PaymentStatus.Paid)
            .Select(b => new MyTicketResponse
            {
                BookingId = b.Id,
                BookingCode = b.UniqueCode,
                EventTitle = b.Event!.Title,
                EventDate = b.Event.EventDate,
                VenueName = b.Event.Venue != null ? b.Event.Venue.Name : null,
                VenueCity = b.Event != null && b.Event.Venue != null && b.Event.Venue.City != null ? b.Event.Venue.City.Name : null,
                Quantity = b.Quantity,
                TotalAmount = b.TotalAmount,
                CreatedAt = b.CreatedAt,
                Tickets = b.Tickets.Select(t => new TicketDto
                {
                    Id = t.Id,
                    TicketCode = t.TicketCode,
                    QrCodeUrl = t.QrCodePath != null ? $"{t.QrCodePath}" : null,
                    PdfUrl = t.PdfPath != null ? $"{t.PdfPath}" : null,
                    IsUsed = t.IsUsed,
                    UsedAt = t.UsedAt,
                }).ToList()
            });

        int totalCount = await query.CountAsync();

        List<MyTicketResponse> items = await query
            .OrderBy(b => b.EventDate)
            .Skip((pageRequest.PageNumber - 1) * pageRequest.PageSize)
            .Take(pageRequest.PageSize)
            .ToListAsync();

        return new PagedResult<MyTicketResponse>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<bool> HasBookingsAsync(int eventId)
    {
        return await _context.Bookings.AnyAsync(b => b.EventId == eventId);
    }

    public async Task<(List<EventAttendeeDto> Items, int TotalCount)> GetPagedBookingsByOrganizerIdAsync(int organizerId, int pageNumber, int pageSize)
    {
        IQueryable<Booking> query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.Event!.OrganizerId == organizerId);

        int totalCount = await query.CountAsync();

        List<EventAttendeeDto> items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new EventAttendeeDto
            {
                BookingId = b.Id,
                UserId = b.UserId,
                CustomerName = b.User != null ? b.User.Name: string.Empty,
                CustomerEmail = b.User != null ? b.User.Email : string.Empty,
                CustomerPhone = b.User != null ? b.User.Phone : null,
                EventId = b.EventId,
                EventTitle = b.Event != null ? b.Event.Title : string.Empty,
                Quantity = b.Quantity,
                TotalAmount = b.TotalAmount,
                PaymentStatus = b.PaymentStatus.ToString(),
                BookedAt = b.CreatedAt
            })
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Booking>> GetBookingsByOrganizerIdAsync(int organizerId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Event!.OrganizerId == organizerId)
            .Include(b => b.User)
            .Include(b => b.Event).ThenInclude(e => e!.Category)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }
}