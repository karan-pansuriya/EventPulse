using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Authorize(Policy = "CustomerOnly")]
[Route("api/tickets")]
public class TicketsController : BaseHelper
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("my-tickets/{bookingId}")]
    public async Task<IActionResult> GetMyTickets(int bookingId)
    {
        List<MyTicketResponse> result = await _ticketService.GetMyTicketsAsync(bookingId);
        return SuccessResponse(result);
    }

    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetAllMyTickets()
    {
        List<MyTicketResponse> result = await _ticketService.GetAllMyTicketsAsync();
        return SuccessResponse(result);
    }

    [HttpGet("download/{ticketId}")]
    public async Task<IActionResult> DownloadTicket(int ticketId)
    {
        var (fullPath, ticketCode) = await _ticketService.GetTicketDownloadInfoAsync(ticketId);
        return PhysicalFile(fullPath, "application/pdf", $"ticket-{ticketCode}.pdf");
    }
}
