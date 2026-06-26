using EventPulse.BLL.DTOs.Booking;
using EventPulse.Common.Models.Response;
using EventPulse.Common.Models;
using EventPulse.DAL.Entities;
using EventPulse.BLL.DTOs.Event;

namespace EventPulse.DAL.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<Event?> GetEventByIdAsync(int eventId);

    Task<Booking> CreatePendingBookingAsync(
        int userId, int eventId, string uniqueCode, int quantity,
        decimal pricePerTicket, decimal totalAmount, string paymentIntentId);

    Task<(Booking Booking, int RemainingSeats)> ConfirmPaymentAsync(string paymentIntentId);

    Task MarkPaymentFailedAsync(string paymentIntentId);

    Task<BookingResponse?> GetByPaymentIntentAsync(string paymentIntentId);

    Task<Booking?> GetBookingWithDetailsAsync(int bookingId);

    Task<Booking?> GetBookingByTicketIdAsync(int ticketId);

    Task<TicketCheckInProjection?> GetTicketByCodeAsync(string ticketCode);

    Task MarkTicketAsUsedAsync(int ticketId);

    Task UpdateTicketPathsAsync(ICollection<DAL.Entities.Ticket> tickets);

    Task<List<MyTicketResponse>> GetUserBookingsAsync(int userId, int bookingId);

    Task<PagedResult<MyTicketResponse>> GetPagedUserBookingsAsync(int userId, PageRequest pageRequest);

    Task<List<BookingProjection>> GetAllBookingsAsync();

    Task<PagedResult<AdminBookingResponse>> GetPagedBookingsAsync(PageRequest pageRequest);

    Task<bool> HasBookingsAsync(int eventId);

    Task<(List<EventAttendeeDto> Items, int TotalCount)> GetPagedBookingsByOrganizerIdAsync(int organizerId, int pageNumber, int pageSize);

    Task<List<Booking>> GetBookingsByOrganizerIdAsync(int organizerId);


}