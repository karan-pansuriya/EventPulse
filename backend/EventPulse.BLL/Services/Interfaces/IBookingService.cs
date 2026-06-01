using EventPulse.BLL.DTOs.Booking;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;

namespace EventPulse.BLL.Interfaces;

public interface IBookingService
{
    Task<PagedResult<AdminBookingResponse>> GetPagedBookingsAsync(PageRequest pageRequest);
}
