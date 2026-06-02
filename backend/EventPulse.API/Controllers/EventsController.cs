using EventPulse.API.Helpers;
using EventPulse.BLL.Common;
using EventPulse.BLL.DTOs.Dashboard;
using EventPulse.BLL.DTOs.Event;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Authorize(Policy = "OrganizerOrAdmin")]
[Route("api/events")]
public class EventsController : BaseHelper
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllEventsForAdmin([FromQuery] PageRequest pageRequest)
    {
        PagedResult<EventListResponse> result = await _eventService.GetAllEventsAsync(pageRequest);
        return SuccessResponse(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}/verify")]
    public async Task<IActionResult> ToggleVerification(int id)
    {
        await _eventService.ToggleVerificationAsync(id);
        return SuccessResponse("Event verification status updated.");
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventById(int id)
    {
        EventResponse result = await _eventService.GetByIdAsync(id);
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
    public async Task<IActionResult> GetMyEvents([FromQuery] PageRequest pageRequest)
    {
        PagedResult<EventListResponse> result = await _eventService.GetMyEventsAsync(pageRequest);
        return SuccessResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent([FromForm] CreateEventDto dto, [FromForm] List<IFormFile>? posterImages)
    {
        List<(byte[] ImageBytes, string FileName)> files = await ReadFormFilesAsync(posterImages);
        EventResponse result = await _eventService.CreateAsync(dto, files);
        return CreatedResponse(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromForm] UpdateEventDto dto, [FromForm] List<IFormFile>? posterImages)
    {
        List<(byte[] ImageBytes, string FileName)> files = await ReadFormFilesAsync(posterImages);
        EventResponse result = await _eventService.UpdateAsync(id, dto, files);
        return SuccessResponse(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("admin/dashboard")]
    public async Task<IActionResult> GetAdminDashboard([FromQuery] string? period = "year", [FromQuery] int? organizerId = null)
    {
        OrganizerDashboardDto result = await _eventService.GetAdminDashboardDataAsync(period, organizerId);
        return SuccessResponse(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] string? period = "year")
    {
        OrganizerDashboardDto result = await _eventService.GetDashboardDataAsync(period);
        return SuccessResponse(result);
    }

    [HttpGet("attendees")]
    public async Task<IActionResult> GetAttendees()
    {
        List<EventAttendeeDto> result = await _eventService.GetAttendeesAsync();
        return SuccessResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        await _eventService.DeleteAsync(id);
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