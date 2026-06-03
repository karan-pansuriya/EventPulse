using EventPulse.BLL.DTOs.Dashboard;
using EventPulse.BLL.DTOs.Event;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;

namespace EventPulse.BLL.Interfaces;

public interface IEventService
{
    Task<EventResponse> GetByIdAsync(int id);
    Task<PagedResult<EventListResponse>> GetPagedAsync(EventFilterRequest filter);
    Task<PagedResult<EventListResponse>> GetMyEventsAsync(PageRequest pageRequest);
    Task<PagedResult<EventListResponse>> GetAllEventsAsync(PageRequest pageRequest);
    Task<PagedResult<EventAttendeeDto>> GetAttendeesAsync(int pageNumber = 1, int pageSize = 10);
    Task<OrganizerDashboardDto> GetDashboardDataAsync(string? period = "year");
    Task<OrganizerDashboardDto> GetAdminDashboardDataAsync(string? period = "year", int? organizerId = null);
    Task<EventResponse> CreateAsync(CreateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages);
    Task<EventResponse> UpdateAsync(int id, UpdateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages);
    Task DeleteAsync(int id);
    Task ToggleVerificationAsync(int id);
}
