using AutoMapper;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.BLL.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public BookingService(IBookingRepository bookingRepository, IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<BookingResponse> CreateBookingAsync(int userId, CreateBookingDto dto)
    {
        if (dto.Quantity < 1)
            throw new BadRequestException("Quantity must be at least 1.");

        Event eventEntity = await _bookingRepository.GetEventByIdAsync(dto.EventId)
            ?? throw new NotFoundException("Event not found.");

        if (dto.Quantity > eventEntity.TotalSeats)
            throw new BadRequestException($"Only {eventEntity.TotalSeats} seats available.");

        string uniqueCode = GenerateUniqueCode();

        Booking booking;
        int remainingSeats;
        try
        {
            (booking, remainingSeats) = await _bookingRepository.CreateFullBookingAsync(
                userId, dto.EventId, uniqueCode, dto.Quantity, eventEntity.Price, eventEntity.Price * dto.Quantity);
        }
        catch (InvalidOperationException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        Booking? bookingWithDetails = await _bookingRepository.GetBookingWithDetailsAsync(booking.Id);

        BookingResponse response = _mapper.Map<BookingResponse>(bookingWithDetails);
        response.RemainingSeats = remainingSeats;
        return response;
    }

    private static string GenerateUniqueCode()
    {
        return $"BK-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
    }
}
