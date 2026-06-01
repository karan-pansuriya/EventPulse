using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("api/bookings")]
public class BookingsController : BaseHelper
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagedBookings([FromQuery] PageRequest pageRequest)
    {
        PagedResult<AdminBookingResponse> result = await _bookingService.GetPagedBookingsAsync(pageRequest);
        return SuccessResponse(result);
    }
}
