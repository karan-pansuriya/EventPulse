using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.DTOs.Dashboard;
using EventPulse.BLL.DTOs.Event;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;

namespace EventPulse.BLL.Interfaces;

public interface IBookingService
{
    Task<PagedResult<AdminBookingResponse>> GetPagedBookingsAsync(PageRequest pageRequest);

    Task<PagedResult<TopBookedEventDto>> GetTopBookedEventsForOrganizerAsync(PageRequest pageRequest);

    Task<PagedResult<EventAttendeeDto>> GetAttendeesAsync(int pageNumber = 1, int pageSize = 10);


}
