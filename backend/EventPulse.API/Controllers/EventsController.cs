using EventPulse.BLL.DTOs.Event;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Authorize(Roles = "Organizer,Admin")]
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
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.Identity?.IsAuthenticated == true ? GetUserId() : (int?)null;
        var userRole = User.Identity?.IsAuthenticated == true ? (GetUserRoles().Contains("Admin") ? "Admin" : "Organizer") : null;
        var result = await _eventService.GetByIdAsync(id, userId, userRole);
        return SuccessResponse(result);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EventFilterRequest filter)
    {
        var result = await _eventService.GetPagedAsync(filter);
        return SuccessResponse(result);
    }

    [HttpGet("my-events")]
    public async Task<IActionResult> GetMyEvents([FromQuery] PageRequest pageRequest)
    {
        var userId = GetUserId();
        var result = await _eventService.GetMyEventsAsync(userId, pageRequest);
        return SuccessResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        var userId = GetUserId();
        var result = await _eventService.CreateAsync(userId, dto, null);
        return CreatedResponse(result);
    }

    [HttpPost("with-posters")]
    public async Task<IActionResult> CreateWithPosters([FromForm] CreateEventDto dto, [FromForm] List<IFormFile>? posterImages)
    {
        var userId = GetUserId();
        var files = await ReadFormFilesAsync(posterImages);
        var result = await _eventService.CreateAsync(userId, dto, files);
        return CreatedResponse(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEventDto dto)
    {
        var userId = GetUserId();
        var role = GetUserRoles().Contains("Admin") ? "Admin" : "Organizer";
        var result = await _eventService.UpdateAsync(id, userId, role, dto, null);
        return SuccessResponse(result);
    }

    [HttpPut("{id}/with-posters")]
    public async Task<IActionResult> UpdateWithPosters(int id, [FromForm] UpdateEventDto dto, [FromForm] List<IFormFile>? posterImages)
    {
        var userId = GetUserId();
        var role = GetUserRoles().Contains("Admin") ? "Admin" : "Organizer";
        var files = await ReadFormFilesAsync(posterImages);
        var result = await _eventService.UpdateAsync(id, userId, role, dto, files);
        return SuccessResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var role = GetUserRoles().Contains("Admin") ? "Admin" : "Organizer";
        await _eventService.DeleteAsync(id, userId, role);
        return SuccessResponse("Event deleted successfully.");
    }

    private static async Task<List<(byte[] ImageBytes, string FileName)>> ReadFormFilesAsync(List<IFormFile>? files)
    {
        var result = new List<(byte[] ImageBytes, string FileName)>();

        if (files is null || files.Count == 0)
            return result;

        foreach (var file in files)
        {
            if (file is null || file.Length == 0)
                continue;

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            result.Add((ms.ToArray(), file.FileName));
        }

        return result;
    }
}
