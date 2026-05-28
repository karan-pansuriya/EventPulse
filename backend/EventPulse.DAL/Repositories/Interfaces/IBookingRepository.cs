using EventPulse.DAL.Entities;

namespace EventPulse.DAL.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<Event?> GetEventByIdAsync(int eventId);

    Task<Booking> CreatePendingBookingAsync(
        int userId, int eventId, string uniqueCode, int quantity,
        decimal pricePerTicket, decimal totalAmount, string paymentIntentId);

    Task<(Booking Booking, int RemainingSeats)> ConfirmPaymentAsync(string paymentIntentId);

    Task MarkPaymentFailedAsync(string paymentIntentId);

    Task<Booking?> GetByPaymentIntentAsync(string paymentIntentId);

    Task<Booking?> GetBookingWithDetailsAsync(int bookingId);
}
