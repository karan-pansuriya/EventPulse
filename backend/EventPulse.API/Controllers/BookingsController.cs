using EventPulse.API.Hubs;
using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace EventPulse.API.Controllers;

[Authorize(Roles = "Customer")]
[Route("api/bookings")]
public class BookingsController : BaseHelper
{
    private readonly IBookingService _bookingService;
    private readonly IHubContext<SeatHub> _hubContext;

    public BookingsController(IBookingService bookingService, IHubContext<SeatHub> hubContext)
    {
        _bookingService = bookingService;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
    {
        int userId = GetUserId();
        BookingResponse result = await _bookingService.CreateBookingAsync(userId, dto);

        await _hubContext.Clients.Group($"event-{dto.EventId}")
            .SendAsync("SeatUpdated", new
            {
                eventId = dto.EventId,
                remainingSeats = result.RemainingSeats,
            });

        return CreatedResponse(result);
    }
}
