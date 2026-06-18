using EventPulse.BLL.DTOs.Booking;
using EventPulse.Common.Models.Response;
using EventPulse.Common.Models;
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

    Task<Booking?> GetBookingByTicketIdAsync(int ticketId);

    Task<Ticket?> GetTicketByCodeAsync(string ticketCode);

    Task MarkTicketAsUsedAsync(Ticket ticket);

    Task UpdateTicketPathsAsync(ICollection<DAL.Entities.Ticket> tickets);

    Task<List<Booking>> GetUserBookingsAsync(int userId, int bookingId);

    Task<List<Booking>> GetUserAllBookingsAsync(int userId);

    Task<PagedResult<Booking>> GetPagedUserBookingsAsync(int userId, PageRequest pageRequest);

    Task<List<BookingProjection>> GetAllBookingsAsync();

    Task<PagedResult<PagedBookingProjection>> GetPagedBookingsAsync(PageRequest pageRequest);

    Task<bool> HasBookingsAsync(int eventId);
}