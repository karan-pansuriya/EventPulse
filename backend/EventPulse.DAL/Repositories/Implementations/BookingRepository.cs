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

    public async Task<(Booking Booking, int RemainingSeats)> CreateFullBookingAsync(
        int userId, int eventId, string uniqueCode, int quantity,
        decimal pricePerTicket, decimal totalAmount)
    {
        await using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Detach any previously tracked event entity so FindAsync loads fresh DB data
            Event? tracked = _context.ChangeTracker.Entries<Event>()
                .Select(e => e.Entity)
                .FirstOrDefault(e => e.Id == eventId);
            if (tracked != null)
                _context.Entry(tracked).State = EntityState.Detached;

            // Pessimistic row lock: block concurrent transactions on this event row
            await _context.Database.ExecuteSqlRawAsync(
                "SELECT 1 FROM events WHERE id = {0} AND NOT is_deleted FOR UPDATE", eventId);

            Event eventEntity = await _context.Events.FindAsync(eventId)
                ?? throw new InvalidOperationException("Event not found.");

            if (quantity > eventEntity.TotalSeats)
                throw new InvalidOperationException($"Only {eventEntity.TotalSeats} seats available.");

            eventEntity.TotalSeats -= quantity;

            Booking booking = new Booking
            {
                UserId = userId,
                EventId = eventId,
                UniqueCode = uniqueCode,
                Quantity = quantity,
                PricePerTicket = pricePerTicket,
                TotalAmount = totalAmount,
                PaymentStatus = PaymentStatus.Paid,
                BookingStatus = BookingStatus.Confirmed,
            };

            for (int i = 0; i < quantity; i++)
            {
                booking.Tickets.Add(new Ticket
                {
                    TicketCode = $"{uniqueCode}-{i + 1}",
                });
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (booking, eventEntity.TotalSeats);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
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
