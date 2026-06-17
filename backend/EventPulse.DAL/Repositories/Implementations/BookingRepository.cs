using EventPulse.BLL.DTOs.Booking;
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
            .FirstOrDefaultAsync(e => e.Id == eventId && !e.IsDeleted);
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

    public async Task<Booking?> GetByPaymentIntentAsync(string paymentIntentId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Event)
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.PaymentRef == paymentIntentId);
    }

    public async Task<Booking?> GetBookingByTicketIdAsync(int ticketId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Tickets.Any(t => t.Id == ticketId))
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync();
    }

    public async Task<Ticket?> GetTicketByCodeAsync(string ticketCode)
    {
        return await _context.Tickets
            .Include(t => t.Booking)
                .ThenInclude(b => b.Event)
            .Include(t => t.Booking)
                .ThenInclude(b => b.User)
            .FirstOrDefaultAsync(t => t.TicketCode == ticketCode);
    }

    public async Task MarkTicketAsUsedAsync(Ticket ticket)
    {
        Ticket? tracked = _context.ChangeTracker.Entries<Ticket>()
            .Select(e => e.Entity)
            .FirstOrDefault(e => e.Id == ticket.Id);

        if (tracked != null)
        {
            tracked.IsUsed = ticket.IsUsed;
            tracked.UsedAt = ticket.UsedAt;
        }

        await _context.SaveChangesAsync();
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

    public async Task<List<Booking>> GetUserBookingsAsync(int userId, int bookingId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.PaymentStatus == PaymentStatus.Paid && b.Id == bookingId)
            .Include(b => b.Event)!.ThenInclude(e => e!.Venue)
            .Include(b => b.Tickets)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<BookingProjection>> GetAllBookingsAsync()
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.PaymentStatus == PaymentStatus.Paid && !b.IsDeleted)
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

    public async Task<PagedResult<PagedBookingProjection>> GetPagedBookingsAsync(PageRequest pageRequest)
    {
        IQueryable<PagedBookingProjection> query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.PaymentStatus == PaymentStatus.Paid && !b.IsDeleted)
            .Select(b => new PagedBookingProjection
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
                PaymentStatus = b.PaymentStatus,
                UniqueCode = b.UniqueCode,
                CreatedAt = b.CreatedAt,
            });

        int totalCount = await query.CountAsync();

        List<PagedBookingProjection> items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageRequest.PageNumber - 1) * pageRequest.PageSize)
            .Take(pageRequest.PageSize)
            .ToListAsync();

        return new PagedResult<PagedBookingProjection>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<List<Booking>> GetUserAllBookingsAsync(int userId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.PaymentStatus == PaymentStatus.Paid && b.IsDeleted == false)
            .Include(b => b.Event)!.ThenInclude(e => e!.Venue)
            .Include(b => b.Tickets)
            .OrderBy(b => b.Event!.EventDate)
            .ToListAsync();
    }

    public async Task<PagedResult<Booking>> GetPagedUserBookingsAsync(int userId, PageRequest pageRequest)
    {
        IQueryable<Booking> query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId && b.PaymentStatus == PaymentStatus.Paid && !b.IsDeleted)
            .Include(b => b.Event)!.ThenInclude(e => e!.Venue)
            .Include(b => b.Tickets);

        int totalCount = await query.CountAsync();

        List<Booking> items = await query
            .OrderBy(b => b.Event!.EventDate)
            .Skip((pageRequest.PageNumber - 1) * pageRequest.PageSize)
            .Take(pageRequest.PageSize)
            .ToListAsync();

        return new PagedResult<Booking>
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}