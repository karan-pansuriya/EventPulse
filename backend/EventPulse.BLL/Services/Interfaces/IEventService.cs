using EventPulse.BLL.DTOs.Event;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;

namespace EventPulse.BLL.Interfaces;

public interface IEventService
{
    Task<EventResponse> GetByIdAsync(int id, int? userId = null, string? userRole = null);
    Task<PagedResult<EventListResponse>> GetPagedAsync(EventFilterRequest filter);
    Task<PagedResult<EventListResponse>> GetMyEventsAsync(int organizerId, PageRequest pageRequest);
    Task<EventResponse> CreateAsync(int organizerId, CreateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages);
    Task<EventResponse> UpdateAsync(int id, int userId, string userRole, UpdateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages);
    Task DeleteAsync(int id, int userId, string userRole);
}
