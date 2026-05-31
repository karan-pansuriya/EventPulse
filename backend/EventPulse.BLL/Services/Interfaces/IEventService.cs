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
    Task<List<EventAttendeeDto>> GetAttendeesAsync();
    Task<EventResponse> CreateAsync(CreateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages);
    Task<EventResponse> UpdateAsync(int id, UpdateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages);
    Task DeleteAsync(int id);
    Task ToggleVerificationAsync(int id);
}
