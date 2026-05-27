using EventPulse.BLL.DTOs.Booking;

namespace EventPulse.BLL.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateBookingAsync(int userId, CreateBookingDto dto);
}
