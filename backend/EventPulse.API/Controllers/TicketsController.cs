using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Route("api/tickets")]
public class TicketsController : BaseHelper
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [Authorize(Policy = "CustomerOnly")]
    [HttpGet("my-tickets/{bookingId}")]
    public async Task<IActionResult> GetMyTickets(int bookingId)
    {
        List<MyTicketResponse> result = await _ticketService.GetMyTicketsAsync(bookingId);
        return SuccessResponse(result);
    }

    [Authorize(Policy = "CustomerOnly")]
    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetAllMyTickets()
    {
        List<MyTicketResponse> result = await _ticketService.GetAllMyTicketsAsync();
        return SuccessResponse(result);
    }

    [Authorize(Policy = "CustomerOnly")]
    [HttpGet("download/{ticketId}")]
    public async Task<IActionResult> DownloadTicket(int ticketId)
    {
        var (fullPath, ticketCode) = await _ticketService.GetTicketDownloadInfoAsync(ticketId);
        return PhysicalFile(fullPath, "application/pdf", $"ticket-{ticketCode}.pdf");
    }

    [Authorize(Policy = "OrganizerOnly")]
    [HttpPost("check-in")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        CheckInResponse result = await _ticketService.CheckInAsync(request.TicketCode);
        return SuccessResponse(result);
    }
}
