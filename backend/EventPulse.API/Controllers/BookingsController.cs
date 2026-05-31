using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("api/bookings")]
public class BookingsController : BaseHelper
{
    private readonly IBookingRepository _bookingRepository;

    public BookingsController(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    [HttpGet("admin/all")]
    public async Task<IActionResult> GetAllBookings()
    {
        List<Booking> bookings = await _bookingRepository.GetAllBookingsAsync();

        List<AdminBookingResponse> result = bookings.Select(b => new AdminBookingResponse
        {
            Id = b.Id,
            UserId = b.UserId,
            CustomerName = b.User?.Name ?? "Unknown",
            CustomerEmail = b.User?.Email ?? "",
            EventId = b.EventId,
            EventTitle = b.Event?.Title ?? "Unknown",
            VenueName = b.Event?.Venue?.Name ?? "",
            Quantity = b.Quantity,
            TotalAmount = b.TotalAmount,
            PaymentStatus = b.PaymentStatus.ToString(),
            BookingStatus = b.BookingStatus.ToString(),
            UniqueCode = b.UniqueCode,
            CreatedAt = b.CreatedAt,
        }).ToList();

        return SuccessResponse(result);
    }
}
