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
            BookingStatus = BookingStatus.Confirmed,
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

            for (int i = 0; i < existing.Quantity; i++)
            {
                existing.Tickets.Add(new Ticket
                {
                    TicketCode = $"{existing.UniqueCode}-{i + 1}",
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
            .Include(b => b.Event)
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync(b => b.PaymentRef == paymentIntentId);
    }

    public async Task<Booking?> GetBookingWithDetailsAsync(int bookingId)
    {
        return await _context.Bookings
            .Where(b => b.Id == bookingId)
            .Include(b => b.Event)
            .Include(b => b.Tickets)
            .FirstOrDefaultAsync();
    }
}
