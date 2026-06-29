using AutoMapper;
using EventPulse.BLL.DTOs.Booking;
using EventPulse.BLL.DTOs.Dashboard;
using EventPulse.BLL.DTOs.Event;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Enums;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventPulse.BLL.Services;

public class BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor) : BaseService(httpContextAccessor), IBookingService
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;

    private readonly IEventRepository _eventRepository = eventRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<PagedResult<AdminBookingResponse>> GetPagedBookingsAsync(PageRequest pageRequest)
    {
        return await _bookingRepository.GetPagedBookingsAsync(pageRequest);

    }

    public async Task<PagedResult<TopBookedEventDto>> GetTopBookedEventsForOrganizerAsync(PageRequest pageRequest)
    {
        int organizerId = GetUserId();

        List<Event> allEvents = await _eventRepository.GetEventsByOrganizerIdAsync(organizerId);
        List<Booking> allBookings = await _bookingRepository.GetBookingsByOrganizerIdAsync(organizerId);
        List<Booking> allPaidBookings = allBookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).ToList();

        var bookingStats = allPaidBookings
            .GroupBy(b => b.EventId)
            .Select(g => new
            {
                EventId = g.Key,
                TotalBookings = g.Sum(b => b.Quantity),
                RevenueGenerated = g.Sum(b => b.TotalAmount),
            })
            .OrderByDescending(x => x.TotalBookings)
            .ToList();

        List<TopBookedEventDto> topBookedEvents = bookingStats
            .Select(bs =>
            {
                Event? ev = allEvents.FirstOrDefault(e => e.Id == bs.EventId);
                TopBookedEventDto dto = _mapper.Map<TopBookedEventDto>(ev);
                dto.TotalBookings = bs.TotalBookings;
                dto.RevenueGenerated = bs.RevenueGenerated;
                return dto;
            })
            .OfType<TopBookedEventDto>()
            .ToList();

        int totalCount = topBookedEvents.Count;
        List<TopBookedEventDto> paged = topBookedEvents
            .Skip((pageRequest.PageNumber - 1) * pageRequest.PageSize)
            .Take(pageRequest.PageSize)
            .ToList();

        return new PagedResult<TopBookedEventDto>
        {
            Items = paged,
            TotalCount = totalCount,
        };
    }

    public async Task<PagedResult<EventAttendeeDto>> GetAttendeesAsync(int pageNumber = 1, int pageSize = 10)
    {
        int organizerId = GetUserId();

        (List<EventAttendeeDto> bookings, int totalCount) = await _bookingRepository.GetPagedBookingsByOrganizerIdAsync(organizerId, pageNumber, pageSize);

        return new PagedResult<EventAttendeeDto>
        {
            Items = bookings,
            TotalCount = totalCount,
        };
    }
}
