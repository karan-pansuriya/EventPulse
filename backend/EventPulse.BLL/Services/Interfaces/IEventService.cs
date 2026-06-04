using EventPulse.BLL.DTOs.Dashboard;
using EventPulse.BLL.DTOs.Event;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;

namespace EventPulse.BLL.Interfaces;

public interface IEventService
{
    Task<EventResponse> GetEventByIdAsync(int id);
    Task<PagedResult<EventListResponse>> GetPagedEventsAsync(EventFilterRequest filter);
    Task<PagedResult<EventListResponse>> GetMyEventsAsync(PageRequest pageRequest);
    Task<PagedResult<EventListResponse>> GetAllEventsAsync(PageRequest pageRequest);
    Task<PagedResult<EventAttendeeDto>> GetAttendeesAsync(int pageNumber = 1, int pageSize = 10);
    Task<OrganizerDashboardDto> GetOrganizerDashboardDataAsync();
    Task<OrganizerDashboardDto> GetAdminDashboardDataAsync(int? organizerId = null);
    Task<List<MonthlyRevenueDto>> GetOrganizerRevenueTrendAsync(string? period = "year");
    Task<List<MonthlyRevenueDto>> GetAdminRevenueTrendAsync(string? period = "year", int? organizerId = null);
    Task<EventResponse> CreateEventAsync(CreateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages);
    Task<EventResponse> UpdateEventAsync(int id, UpdateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages);
    Task DeleteEventAsync(int id);
    Task ToggleVerificationAsync(int id);
}
