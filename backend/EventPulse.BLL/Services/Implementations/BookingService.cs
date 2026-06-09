using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.BLL.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;

    public BookingService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<PagedResult<AdminBookingResponse>> GetPagedBookingsAsync(PageRequest pageRequest)
    {
        PagedResult<Booking> paged = await _bookingRepository.GetPagedBookingsAsync(pageRequest);

        var result = new PagedResult<AdminBookingResponse>
        {
            Items = paged.Items.Select(b => new AdminBookingResponse
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
                UniqueCode = b.UniqueCode,
                CreatedAt = b.CreatedAt,
            }),
            TotalCount = paged.TotalCount,
        };

        return result;
    }
}
