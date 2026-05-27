using EventPulse.DAL.Entities;

namespace EventPulse.DAL.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<Event?> GetEventByIdAsync(int eventId);

    Task<(Booking Booking, int RemainingSeats)> CreateFullBookingAsync(
        int userId, int eventId, string uniqueCode, int quantity,
        decimal pricePerTicket, decimal totalAmount);

    Task<Booking?> GetBookingWithDetailsAsync(int bookingId);
}
