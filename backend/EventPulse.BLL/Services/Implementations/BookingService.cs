using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
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
        return await _bookingRepository.GetPagedBookingsAsync(pageRequest);
        
    }
}
