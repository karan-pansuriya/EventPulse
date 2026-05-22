using EventPulse.Bll.Helpers;
using EventPulse.BLL.DTOs.Event;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Route("api/events")]
public class EventsController : BaseController
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        int? userId = User.Identity?.IsAuthenticated == true ? GetUserId() : (int?)null;
        string? userRole = User.Identity?.IsAuthenticated == true ? (GetUserRoles().Contains("Admin") ? "Admin" : "Organizer") : null;
        EventResponse result = await _eventService.GetByIdAsync(id, userId, userRole);
        return SuccessResponse(result);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllEvents([FromQuery] EventFilterRequest filter)
    {
        PagedResult<EventListResponse> result = await _eventService.GetPagedAsync(filter);
        return SuccessResponse(result);
    }

    [HttpGet("my-events")]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> GetMyEvents([FromQuery] PageRequest pageRequest)
    {
        int userId = GetUserId();
        PagedResult<EventListResponse> result = await _eventService.GetMyEventsAsync(userId, pageRequest);
        return SuccessResponse(result);
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> CreateEvent([FromForm] CreateEventDto dto, [FromForm] List<IFormFile>? posterImages)
    {
        int userId = GetUserId();
        List<(byte[] ImageBytes, string FileName)> files = await ReadFormFilesAsync(posterImages);
        EventResponse result = await _eventService.CreateAsync(userId, dto, files);
        return CreatedResponse(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> UpdateEvent(int id, [FromForm] UpdateEventDto dto, [FromForm] List<IFormFile>? posterImages)
    {
        int userId = GetUserId();
        string role = GetUserRoles().Contains("Admin") ? "Admin" : "Organizer";
        List<(byte[] ImageBytes, string FileName)> files = await ReadFormFilesAsync(posterImages);
        EventResponse result = await _eventService.UpdateAsync(id, userId, role, dto, files);
        return SuccessResponse(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        int userId = GetUserId();
        string role = GetUserRoles().Contains("Admin") ? "Admin" : "Organizer";
        await _eventService.DeleteAsync(id, userId, role);
        return SuccessResponse("Event deleted successfully.");
    }

    private static async Task<List<(byte[] ImageBytes, string FileName)>> ReadFormFilesAsync(List<IFormFile>? files)
    {
        List<(byte[] ImageBytes, string FileName)> result = [];

        if (files is null || files.Count == 0)
            return result;

        foreach (IFormFile file in files)
        {
            if (file is null || file.Length == 0)
                continue;

            using MemoryStream ms = new();
            await file.CopyToAsync(ms);
            result.Add((ms.ToArray(), file.FileName));
        }

        return result;
    }
}
