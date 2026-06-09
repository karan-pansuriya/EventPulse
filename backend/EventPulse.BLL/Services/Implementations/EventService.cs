using System.Text.Json;
using AutoMapper;
using EventPulse.BLL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using EventPulse.BLL.DTOs.Dashboard;
using EventPulse.BLL.DTOs.Event;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Enums;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.BLL.Services;

public class EventService : BaseService, IEventService
{
    private const int MaxPosterImages = 5;
    private const long MaxPosterFileSize = 5 * 1024 * 1024; // 5 MB
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    private readonly IGenericRepository<Event> _eventRepo;
    private readonly IGenericRepository<EventPoster> _posterRepo;
    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private int _eventsCacheVersion = 0;

    public EventService(
        IGenericRepository<Event> eventRepo,
        IGenericRepository<EventPoster> posterRepo,
        IEventRepository eventRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IImageService imageService,
        IMapper mapper,
        IMemoryCache cache,
        IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
        _eventRepo = eventRepo;
        _posterRepo = posterRepo;
        _eventRepository = eventRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _imageService = imageService;
        _mapper = mapper;
        _cache = cache;
    }

    private string EventsCacheKey<T>(T key)
    {
        int? roleId = GetActiveRoleId();
        return $"events_v{_eventsCacheVersion}_{JsonSerializer.Serialize(key)}";
    }

    private void InvalidateEventsCache()
    {
        Interlocked.Increment(ref _eventsCacheVersion);
    }

    private int? GetActiveRoleId()
    {
        string? value = HttpContextAccessor.HttpContext?.User.FindFirst("active_role_id")?.Value;
        if (int.TryParse(value, out int roleId) && roleId > 0)
            return roleId;
        return null;
    }

    public async Task<EventResponse> GetEventByIdAsync(int id)
    {
        Event eventEntity = await _eventRepository.GetEventWithDetailsAsync(id)
            ?? throw new NotFoundException("Event not found.");

        int? userRoleId = GetActiveRoleId();
        if (userRoleId == RoleId.Organizer)
        {
            int userId = GetUserId();
            if (eventEntity.OrganizerId != userId)
                throw new ForbiddenException("You are not authorized to view this event.");
        }

        return _mapper.Map<EventResponse>(eventEntity);
    }

    public async Task<PagedResult<EventListResponse>> GetCustomerPagedEventsAsync(EventFilterRequest filter)
    {
        string cacheKey = EventsCacheKey(filter);

        if (_cache.TryGetValue<PagedResult<EventListResponse>>(cacheKey, out var cached))
            return cached!;

        (List<Event> items, int totalCount) = await _eventRepository.GetCustomerPagedEventsAsync(filter);

        List<EventListResponse> responseItems = _mapper.Map<List<EventListResponse>>(items);

        var result = new PagedResult<EventListResponse>
        {
            Items = responseItems,
            TotalCount = totalCount
        };

        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<PagedResult<EventListResponse>> GetMyEventsForOrganizerAsync(PageRequest pageRequest)
    {
        int organizerId = GetUserId();

        string cacheKey = EventsCacheKey(new { organizerId, pageRequest });

        if (_cache.TryGetValue<PagedResult<EventListResponse>>(cacheKey, out var cached))
            return cached!;

        pageRequest.SortBy ??= "EventDate";
        pageRequest.SortDirection ??= "desc";

        var result = await _eventRepo.GetPagedAsync(
            e => e.OrganizerId == organizerId && !e.IsDeleted,
            e => new EventListResponse
            {
                Id = e.Id,
                Title = e.Title,
                CategoryName = e.Category != null ? e.Category.Name : null,
                VenueId = e.VenueId,
                VenueName = e.Venue != null ? e.Venue.Name : null,
                EventDate = e.EventDate,
                StartTime = e.StartTime,
                Price = e.Price,
                TotalSeats = e.TotalSeats,
                IsVerified = e.IsVerified,
                IsActive = e.IsActive,
                PosterUrl = e.Posters.Select(p => p.PosterUrl).FirstOrDefault(),
            },
            pageRequest);

        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<PagedResult<EventListResponse>> GetAllEventsForAdminAsync(PageRequest pageRequest)
    {
        string cacheKey = EventsCacheKey(pageRequest);

        if (_cache.TryGetValue<PagedResult<EventListResponse>>(cacheKey, out var cached))
            return cached!;

        var result = await _eventRepo.GetPagedAsync(
            e => !e.IsDeleted && e.EventDate >= DateTime.UtcNow.Date,
            e => new EventListResponse
            {
                Id = e.Id,
                Title = e.Title,
                CategoryName = e.Category != null ? e.Category.Name : null,
                VenueId = e.VenueId,
                VenueName = e.Venue != null ? e.Venue.Name : null,
                EventDate = e.EventDate,
                StartTime = e.StartTime,
                Price = e.Price,
                TotalSeats = e.TotalSeats,
                IsVerified = e.IsVerified,
                IsActive = e.IsActive,
                PosterUrl = e.Posters.Select(p => p.PosterUrl).FirstOrDefault(),
            },
            pageRequest);

        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task ToggleVerificationAsync(int id)
    {
        Event? eventEntity = await _eventRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Event not found.");

        eventEntity.IsVerified = !eventEntity.IsVerified;
        _eventRepo.Update(eventEntity);
        await _unitOfWork.SaveAsync();
        InvalidateEventsCache();
    }

    public async Task<PagedResult<EventAttendeeDto>> GetAttendeesAsync(int pageNumber = 1, int pageSize = 10)
    {
        int organizerId = GetUserId();

        var (bookings, totalCount) = await _eventRepository.GetPagedBookingsByOrganizerIdAsync(organizerId, pageNumber, pageSize);

        List<EventAttendeeDto> items = bookings
            .Where(b => b.User != null && b.Event != null)
            .Select(_mapper.Map<EventAttendeeDto>).ToList();

        return new PagedResult<EventAttendeeDto>
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    public async Task<EventResponse> CreateEventAsync(CreateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages)
    {
        int organizerId = GetUserId();

        if (posterImages?.Count > MaxPosterImages)
            throw new BadRequestException($"Maximum {MaxPosterImages} poster images allowed.");

        if (posterImages is { Count: > 0 })
        {
            if (posterImages.Any(f => f.ImageBytes.Length > MaxPosterFileSize))
                throw new BadRequestException($"Each poster image must be 5 MB or less.");
        }

        Event? duplicate = await _eventRepository.GetEventByTitleDateVenueAsync(dto.Title, dto.EventDate, dto.VenueName);
        if (duplicate != null)
            throw new BadRequestException("An event with the same title, date, and venue already exists.");

        Venue? venue = await _eventRepository.ResolveVenueAsync(dto.VenueName, dto.VenueAddress, dto.CityId);
        if (venue != null && venue.Id == 0)
        {
            await _unitOfWork.SaveAsync();
        }

        Event eventEntity = new Event
        {
            OrganizerId = organizerId,
            CategoryId = dto.CategoryId,
            VenueId = venue?.Id,
            Title = dto.Title,
            Description = dto.Description,
            Genre = dto.Genre,
            AgeRestriction = dto.AgeRestriction,
            Performers = dto.Performers,
            DurationMins = dto.DurationMins,
            EventDate = dto.EventDate,
            StartTime = dto.StartTime,
            Price = dto.Price,
            TotalSeats = dto.TotalSeats,
            IsActive = true,
        };

        await _eventRepo.AddAsync(eventEntity);
        await _unitOfWork.SaveAsync();
        InvalidateEventsCache();

        if (posterImages is { Count: > 0 })
        {
            foreach ((byte[] imageBytes, string fileName) in posterImages)
            {
                string relativePath = await _imageService.SaveImageAsync(imageBytes, fileName, "event_poster");
                EventPoster poster = new EventPoster
                {
                    EventId = eventEntity.Id,
                    PosterUrl = relativePath,
                };
                await _posterRepo.AddAsync(poster);
            }
            await _unitOfWork.SaveAsync();
        }

        return await GetEventByIdAsync(eventEntity.Id);
    }

    public async Task<EventResponse> UpdateEventAsync(int id, UpdateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages)
    {
        int userId = GetUserId();
        int userRoleId = GetActiveRoleId() ?? throw new UnauthorizedAccessException("Active role not found.");

        if (posterImages?.Count > MaxPosterImages)
            throw new BadRequestException($"Maximum {MaxPosterImages} poster images allowed.");

        if (posterImages is { Count: > 0 })
        {
            if (posterImages.Any(f => f.ImageBytes.Length > MaxPosterFileSize))
                throw new BadRequestException($"Each poster image must be 5 MB or less.");
        }

        Event? eventEntity = await _eventRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Event not found.");

        if (userRoleId != RoleId.Admin && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to update this event.");

        if (eventEntity.IsVerified)
            throw new ForbiddenException("Verified events cannot be edited.");

        if (eventEntity.EventDate < DateTime.Today)
            throw new BadRequestException("Past events cannot be edited.");

        Venue? venue = await _eventRepository.ResolveVenueAsync(dto.VenueName, dto.VenueAddress, dto.CityId);
        if (venue != null && venue.Id == 0)
        {
            await _unitOfWork.SaveAsync();
        }

        Event? duplicate = await _eventRepository.GetEventByTitleDateVenueAsync(dto.Title, dto.EventDate, dto.VenueName ?? string.Empty);
        if (duplicate != null && duplicate.Id != id)
            throw new BadRequestException("An event with the same title, date, and venue already exists.");

        eventEntity.CategoryId = dto.CategoryId;
        eventEntity.VenueId = venue?.Id;
        eventEntity.Title = dto.Title;
        eventEntity.Description = dto.Description;
        eventEntity.Genre = dto.Genre;
        eventEntity.AgeRestriction = dto.AgeRestriction;
        eventEntity.Performers = dto.Performers;
        eventEntity.DurationMins = dto.DurationMins;
        eventEntity.EventDate = dto.EventDate;
        eventEntity.StartTime = dto.StartTime;
        eventEntity.Price = dto.Price;
        eventEntity.TotalSeats = dto.TotalSeats;

        _eventRepo.Update(eventEntity);

        if (dto.RemovePosterUrls is { Count: > 0 })
        {
            List<EventPoster> allPosters = await _eventRepository.GetActivePostersByEventIdAsync(id);
            List<EventPoster> toRemove = allPosters.Where(p => dto.RemovePosterUrls.Contains(p.PosterUrl)).ToList();

            foreach (EventPoster ep in toRemove)
            {
                _imageService.DeleteImage(ep.PosterUrl);
                _posterRepo.Delete(ep);
            }
        }

        if (posterImages is { Count: > 0 })
        {
            foreach ((byte[] imageBytes, string fileName) in posterImages)
            {
                string relativePath = await _imageService.SaveImageAsync(imageBytes, fileName, "event_poster");
                EventPoster poster = new EventPoster
                {
                    EventId = id,
                    PosterUrl = relativePath,
                };
                await _posterRepo.AddAsync(poster);
            }
        }

        await _unitOfWork.SaveAsync();
        InvalidateEventsCache();

        return await GetEventByIdAsync(id);
    }

    public async Task DeleteEventAsync(int id)
    {
        int userId = GetUserId();
        int userRoleId = GetActiveRoleId() ?? throw new UnauthorizedAccessException("Active role not found.");

        Event? eventEntity = await _eventRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Event not found.");

        if (userRoleId != RoleId.Admin && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to delete this event.");

        if (eventEntity.EventDate < DateTime.Today)
            throw new BadRequestException("Past events cannot be deleted.");

        int bookedCount = await _eventRepository.GetBookingCountByEventIdAsync(id);
        if (bookedCount >= 1)
            throw new BadRequestException("Cannot delete event with booked tickets.");

        List<EventPoster> posters = await _eventRepository.GetActivePostersByEventIdAsync(id);

        foreach (EventPoster poster in posters)
        {
            _imageService.DeleteImage(poster.PosterUrl);
            _posterRepo.Delete(poster);
        }

        _eventRepo.Delete(eventEntity);
        await _unitOfWork.SaveAsync();
        InvalidateEventsCache();
    }

    public async Task<OrganizerDashboardDto> GetOrganizerDashboardDataAsync()
    {
        int organizerId = GetUserId();

        DateTime now = DateTime.Today;
        DateTime periodStart = new DateTime(now.Year, 1, 1);
        DateTime prevPeriodStart = periodStart.AddYears(-1);

        List<Event> allEvents = await _eventRepository.GetEventsByOrganizerIdAsync(organizerId);
        List<Booking> allBookings = await _eventRepository.GetBookingsByOrganizerIdAsync(organizerId);
        List<Booking> allPaidBookings = allBookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).ToList();

        List<Event> periodEvents = allEvents.Where(e => e.CreatedAt >= periodStart).ToList();
        List<Booking> periodPaidBookings = allPaidBookings.Where(b => b.CreatedAt >= periodStart).ToList();

        int totalEvents = periodEvents.Count;
        int upcomingEvents = periodEvents.Count(e => e.EventDate >= DateTime.Today);
        int totalTicketsSold = periodPaidBookings.Sum(b => b.Quantity);
        decimal totalRevenue = periodPaidBookings.Sum(b => b.TotalAmount);
        int totalAttendees = periodPaidBookings.Select(b => b.UserId).Distinct().Count();

        List<Event> prevPeriodEvents = allEvents.Where(e => e.CreatedAt >= prevPeriodStart && e.CreatedAt < periodStart).ToList();
        List<Booking> prevPeriodPaidBookings = allPaidBookings.Where(b => b.CreatedAt >= prevPeriodStart && b.CreatedAt < periodStart).ToList();

        int prevTotalEvents = prevPeriodEvents.Count;
        int prevUpcomingEvents = prevPeriodEvents.Count(e => e.EventDate >= DateTime.Today);
        int prevTicketsSold = prevPeriodPaidBookings.Sum(b => b.Quantity);
        decimal prevRevenue = prevPeriodPaidBookings.Sum(b => b.TotalAmount);
        int prevAttendees = prevPeriodPaidBookings.Select(b => b.UserId).Distinct().Count();

        List<MonthlyRevenueDto> monthlyRevenue = [];

        List<CategoryEventCountDto> eventsByCategory = periodEvents
            .Where(e => e.Category != null)
            .GroupBy(e => e.Category!.Name)
            .Select(g => new CategoryEventCountDto
            {
                CategoryName = g.Key,
                Count = g.Count(),
            })
            .ToList();

        // Top booked events
        var bookingStats = periodPaidBookings
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
                if (ev == null) return null;
                TopBookedEventDto dto = _mapper.Map<TopBookedEventDto>(ev);
                dto.TotalBookings = bs.TotalBookings;
                dto.RevenueGenerated = bs.RevenueGenerated;
                return dto;
            })
            .OfType<TopBookedEventDto>()
            .ToList();

        List<DashboardRecentAttendeeDto> recentAttendees = periodPaidBookings
            .Where(b => b.User != null && b.Event != null)
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .Select(b => new DashboardRecentAttendeeDto
            {
                BookingId = b.Id,
                UserId = b.UserId,
                CustomerName = b.User.Name,
                CustomerEmail = b.User.Email,
                CustomerPhone = b.User.Phone,
                EventId = b.EventId,
                EventTitle = b.Event.Title,
                Quantity = b.Quantity,
                TotalAmount = b.TotalAmount,
                BookedAt = b.CreatedAt,
            })
            .ToList();

        return new OrganizerDashboardDto
        {
            TotalEvents = totalEvents,
            UpcomingEvents = upcomingEvents,
            TotalTicketsSold = totalTicketsSold,
            TotalRevenue = totalRevenue,
            TotalAttendees = totalAttendees,
            WeeklyComparison = new WeeklyComparisonDto
            {
                EventsChange = prevTotalEvents > 0 ? (totalEvents - prevTotalEvents) * 100 / prevTotalEvents : (totalEvents > 0 ? 100 : 0),
                UpcomingEventsChange = prevUpcomingEvents > 0 ? (upcomingEvents - prevUpcomingEvents) * 100 / prevUpcomingEvents : (upcomingEvents > 0 ? 100 : 0),
                TicketsSoldChange = prevTicketsSold > 0 ? (totalTicketsSold - prevTicketsSold) * 100 / prevTicketsSold : (totalTicketsSold > 0 ? 100 : 0),
                RevenueChange = prevRevenue > 0 ? (totalRevenue - prevRevenue) * 100m / prevRevenue : (totalRevenue > 0 ? 100m : 0m),
                AttendeesChange = prevAttendees > 0 ? (totalAttendees - prevAttendees) * 100 / prevAttendees : (totalAttendees > 0 ? 100 : 0),
            },
            EventsByCategory = eventsByCategory,
            MonthlyRevenue = monthlyRevenue,
            RecentAttendees = recentAttendees,
            TopBookedEvents = topBookedEvents,
        };
    }

    public async Task<OrganizerDashboardDto> GetAdminDashboardDataAsync(int? organizerId = null)
    {
        DateTime now = DateTime.Today;
        DateTime periodStart = new DateTime(now.Year, 1, 1);
        DateTime prevPeriodStart = periodStart.AddYears(-1);

        List<Event> allEvents = organizerId.HasValue
            ? await _eventRepository.GetEventsByOrganizerIdAsync(organizerId.Value)
            : await _eventRepository.GetAllEventsWithDetailsAsync();

        List<Booking> allBookings = organizerId.HasValue
            ? await _eventRepository.GetBookingsByOrganizerIdAsync(organizerId.Value)
            : await _bookingRepository.GetAllBookingsAsync();

        List<Booking> allPaidBookings = allBookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).ToList();

        List<Event> periodEvents = allEvents.Where(e => e.CreatedAt >= periodStart).ToList();
        List<Booking> periodPaidBookings = allPaidBookings.Where(b => b.CreatedAt >= periodStart).ToList();

        int totalEvents = periodEvents.Count;
        int upcomingEvents = periodEvents.Count(e => e.EventDate >= DateTime.Today);
        int totalTicketsSold = periodPaidBookings.Sum(b => b.Quantity);
        decimal totalRevenue = periodPaidBookings.Sum(b => b.TotalAmount);
        int totalAttendees = periodPaidBookings.Select(b => b.UserId).Distinct().Count();

        List<Event> prevPeriodEvents = allEvents.Where(e => e.CreatedAt >= prevPeriodStart && e.CreatedAt < periodStart).ToList();
        List<Booking> prevPeriodPaidBookings = allPaidBookings.Where(b => b.CreatedAt >= prevPeriodStart && b.CreatedAt < periodStart).ToList();

        int prevTotalEvents = prevPeriodEvents.Count;
        int prevUpcomingEvents = prevPeriodEvents.Count(e => e.EventDate >= DateTime.Today);
        int prevTicketsSold = prevPeriodPaidBookings.Sum(b => b.Quantity);
        decimal prevRevenue = prevPeriodPaidBookings.Sum(b => b.TotalAmount);
        int prevAttendees = prevPeriodPaidBookings.Select(b => b.UserId).Distinct().Count();

        List<MonthlyRevenueDto> monthlyRevenue = [];

        List<CategoryEventCountDto> eventsByCategory = periodEvents
            .Where(e => e.Category != null)
            .GroupBy(e => e.Category!.Name)
            .Select(g => new CategoryEventCountDto
            {
                CategoryName = g.Key,
                Count = g.Count(),
            })
            .ToList();

        List<TopBookedEventDto> topBookedEvents = periodPaidBookings
            .GroupBy(b => b.EventId)
            .Select(g => new
            {
                EventId = g.Key,
                TotalBookings = g.Sum(b => b.Quantity),
                RevenueGenerated = g.Sum(b => b.TotalAmount),
            })
            .OrderByDescending(x => x.TotalBookings)
            .ToList()
            .Select(bs =>
            {
                Event? ev = allEvents.FirstOrDefault(e => e.Id == bs.EventId);
                if (ev == null) return null;
                TopBookedEventDto dto = _mapper.Map<TopBookedEventDto>(ev);
                dto.TotalBookings = bs.TotalBookings;
                dto.RevenueGenerated = bs.RevenueGenerated;
                return dto;
            })
            .OfType<TopBookedEventDto>()
            .ToList();

        return new OrganizerDashboardDto
        {
            TotalEvents = totalEvents,
            UpcomingEvents = upcomingEvents,
            TotalTicketsSold = totalTicketsSold,
            TotalRevenue = totalRevenue,
            TotalAttendees = totalAttendees,
            WeeklyComparison = new WeeklyComparisonDto
            {
                EventsChange = prevTotalEvents > 0 ? (totalEvents - prevTotalEvents) * 100 / prevTotalEvents : (totalEvents > 0 ? 100 : 0),
                UpcomingEventsChange = prevUpcomingEvents > 0 ? (upcomingEvents - prevUpcomingEvents) * 100 / prevUpcomingEvents : (upcomingEvents > 0 ? 100 : 0),
                TicketsSoldChange = prevTicketsSold > 0 ? (totalTicketsSold - prevTicketsSold) * 100 / prevTicketsSold : (totalTicketsSold > 0 ? 100 : 0),
                RevenueChange = prevRevenue > 0 ? (totalRevenue - prevRevenue) * 100m / prevRevenue : (totalRevenue > 0 ? 100m : 0m),
                AttendeesChange = prevAttendees > 0 ? (totalAttendees - prevAttendees) * 100 / prevAttendees : (totalAttendees > 0 ? 100 : 0),
            },
            TopBookedEvents = topBookedEvents,
            RecentAttendees = [],
            MonthlyRevenue = monthlyRevenue,
            EventsByCategory = eventsByCategory,
        };
    }

    public async Task<List<MonthlyRevenueDto>> GetOrganizerRevenueTrendAsync(string? period = "year")
    {
        int organizerId = GetUserId();
        List<Booking> bookings = await _eventRepository.GetBookingsByOrganizerIdAsync(organizerId);
        List<Booking> paidBookings = bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).ToList();
        return BuildMonthlyRevenue(paidBookings, period);
    }

    public async Task<List<MonthlyRevenueDto>> GetAdminRevenueTrendAsync(string? period = "year", int? organizerId = null)
    {
        List<Booking> bookings = organizerId.HasValue
            ? await _eventRepository.GetBookingsByOrganizerIdAsync(organizerId.Value)
            : await _bookingRepository.GetAllBookingsAsync();
        List<Booking> paidBookings = bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).ToList();
        return BuildMonthlyRevenue(paidBookings, period);
    }

    private static List<MonthlyRevenueDto> BuildMonthlyRevenue(List<Booking> paidBookings, string? period)
    {
        string periodKey = period?.ToLower() ?? "year";

        if (periodKey == "week")
        {
            string[] dayNames = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
            DateTime weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            return paidBookings
                .Where(b => b.CreatedAt >= weekStart)
                .GroupBy(b => b.CreatedAt.DayOfWeek)
                .Select(g => new MonthlyRevenueDto
                {
                    Month = dayNames[(int)g.Key],
                    Revenue = g.Sum(b => b.TotalAmount),
                    Bookings = g.Sum(b => b.Quantity),
                })
                .OrderBy(m => Array.IndexOf(dayNames, m.Month))
                .ToList();
        }

        if (periodKey == "month")
        {
            DateTime now = DateTime.Today;
            return paidBookings
                .Where(b => b.CreatedAt.Year == now.Year && b.CreatedAt.Month == now.Month)
                .GroupBy(b => b.CreatedAt.Day)
                .Select(g => new MonthlyRevenueDto
                {
                    Month = $"{now:MMM} {g.Key}",
                    Revenue = g.Sum(b => b.TotalAmount),
                    Bookings = g.Sum(b => b.Quantity),
                })
                .OrderBy(m => m.Month)
                .ToList();
        }

        return paidBookings
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Revenue = g.Sum(b => b.TotalAmount),
                Bookings = g.Sum(b => b.Quantity),
            })
            .OrderBy(m => m.Month)
            .ToList();
    }
}
