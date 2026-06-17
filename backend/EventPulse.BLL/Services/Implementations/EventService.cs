using System.Text.Json;
using AutoMapper;
using EventPulse.BLL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using EventPulse.BLL.DTOs.Booking;
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

    private static string EventCacheKey(int eventId) => $"event_{eventId}";

    // Tracker keys — each holds a HashSet<string> of registered cache keys for that group
    private const string CustomerCacheTrackerKey = "tracker_events_customer";
    private const string AdminCacheTrackerKey = "tracker_events_admin";
    private const string OrganizerCacheTrackerKey = "tracker_events_organizer";

    // Registers a cache key into the named tracker so it can be bulk-invalidated later.
    private void TrackCacheKey(string trackerKey, string cacheKey)
    {
        var keys = _cache.GetOrCreate(trackerKey, entry =>
        {
            entry.Priority = CacheItemPriority.NeverRemove;
            return new HashSet<string>();
        })!;
        keys.Add(cacheKey);
    }

    // Removes every cache entry registered under the given tracker, then clears the tracker.
    private void InvalidateTrackedKeys(string trackerKey)
    {
        if (_cache.TryGetValue<HashSet<string>>(trackerKey, out var keys) && keys != null)
        {
            foreach (string key in keys)
                _cache.Remove(key);
            keys.Clear();
        }
    }

    // Invalidates all event-list caches after any mutating operation.
    // Removes customer list, admin list, and the affected organizer's list.
    private void InvalidateListCaches(int? organizerId = null)
    {
        InvalidateTrackedKeys(CustomerCacheTrackerKey);
        InvalidateTrackedKeys(AdminCacheTrackerKey);
        if (organizerId.HasValue)
        {
            InvalidateTrackedKeys($"{OrganizerCacheTrackerKey}_{organizerId.Value}");
        }
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
        int? userRoleId = GetActiveRoleId();
        if (userRoleId == RoleId.Organizer)
        {
            int userId = GetUserId();
            Event? eventEntity = await _eventRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("Event not found.");
            if (eventEntity.OrganizerId != userId)
                throw new ForbiddenException("You are not authorized to view this event.");
        }

        string cacheKey = EventCacheKey(id);
        if (_cache.TryGetValue<EventResponse>(cacheKey, out var cached))
            return cached!;

        Event full = await _eventRepository.GetEventWithDetailsAsync(id)
            ?? throw new NotFoundException("Event not found.");

        var result = _mapper.Map<EventResponse>(full);
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }
    public async Task<PagedResult<EventListResponse>> GetCustomerPagedEventsAsync(EventFilterRequest filter)
    {
        string cacheKey = $"events_customer_{JsonSerializer.Serialize(filter)}";

        if (_cache.TryGetValue<PagedResult<EventListResponse>>(cacheKey, out var cached))
            return cached!;

        (List<Event> items, int totalCount) = await _eventRepository.GetCustomerPagedEventsAsync(filter);

        List<EventListResponse> responseItems = _mapper.Map<List<EventListResponse>>(items);

        var result = new PagedResult<EventListResponse>
        {
            Items = responseItems,
            TotalCount = totalCount
        };

        TrackCacheKey(CustomerCacheTrackerKey, cacheKey);
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<PagedResult<EventListResponse>> GetMyEventsForOrganizerAsync(PageRequest pageRequest)
    {
        int organizerId = GetUserId();

        pageRequest.SortBy ??= "EventDate";
        pageRequest.SortDirection ??= "asc";

        string cacheKey = $"events_organizer_{organizerId}_{JsonSerializer.Serialize(pageRequest)}";

        if (_cache.TryGetValue<PagedResult<EventListResponse>>(cacheKey, out var cached))
            return cached!;

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

        TrackCacheKey($"{OrganizerCacheTrackerKey}_{organizerId}", cacheKey);
        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }

    public async Task<PagedResult<EventListResponse>> GetAllEventsForAdminAsync(PageRequest pageRequest)
    {
        string cacheKey = $"events_admin_{JsonSerializer.Serialize(pageRequest)}";

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

        TrackCacheKey(AdminCacheTrackerKey, cacheKey);
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
        _cache.Remove(EventCacheKey(id));
        InvalidateListCaches(eventEntity.OrganizerId);
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
        int userRoleId = GetActiveRoleId() ?? throw new UnauthorizedAccessException("Active role not found.");
        int organizerId = userRoleId == RoleId.Admin && dto.OrganizerId.HasValue
            ? dto.OrganizerId.Value
            : GetUserId();

        if (posterImages?.Count > MaxPosterImages)
            throw new BadRequestException($"Maximum {MaxPosterImages} poster images allowed.");

        if (posterImages is { Count: > 0 })
        {
            if (posterImages.Any(f => f.ImageBytes.Length > MaxPosterFileSize))
                throw new BadRequestException($"Each poster image must be 5 MB or less.");
        }

        if (dto.EventDate.Date == DateTime.Today && dto.StartTime <= DateTime.Now.TimeOfDay)
            throw new BadRequestException("Event start time must be in the future.");

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

        InvalidateListCaches(organizerId);
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

        if (dto.EventDate.Date == DateTime.Today && dto.StartTime <= DateTime.Now.TimeOfDay)
            throw new BadRequestException("Event start time must be in the future.");

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
        _cache.Remove(EventCacheKey(id));
        InvalidateListCaches(eventEntity.OrganizerId);

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
        _cache.Remove(EventCacheKey(id));
        InvalidateListCaches(eventEntity.OrganizerId);
    }

    private static (DateTime start, DateTime end) GetCurrentWeekBounds()
    {
        DateTime today = DateTime.Today;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime start = today.AddDays(-diff);
        return (start, start.AddDays(7));
    }

    private static int GrowthPercent(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return (int)Math.Round((current - previous) * 100.0 / previous);
    }

    private static decimal GrowthPercent(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100m : 0m;
        return Math.Round((current - previous) * 100m / previous, 1);
    }

    public async Task<OrganizerDashboardDto> GetOrganizerDashboardDataAsync()
    {
        int organizerId = GetUserId();

        var (currentWeekStart, currentWeekEnd) = GetCurrentWeekBounds();
        DateTime lastWeekStart = currentWeekStart.AddDays(-7);

        List<Event> allEvents = await _eventRepository.GetEventsByOrganizerIdAsync(organizerId);
        List<Booking> allBookings = await _eventRepository.GetBookingsByOrganizerIdAsync(organizerId);
        List<Booking> allPaidBookings = allBookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).ToList();

        int totalEvents = allEvents.Count;
        int upcomingEvents = allEvents.Count(e => e.EventDate >= DateTime.Today);
        int totalTicketsSold = allPaidBookings.Sum(b => b.Quantity);
        decimal totalRevenue = allPaidBookings.Sum(b => b.TotalAmount);
        int totalAttendees = allPaidBookings.Select(b => b.UserId).Distinct().Count();

        // Current week
        List<Event> currentWeekEvents = allEvents.Where(e => e.CreatedAt >= currentWeekStart && e.CreatedAt < currentWeekEnd).ToList();
        List<Booking> currentWeekPaid = allPaidBookings.Where(b => b.CreatedAt >= currentWeekStart && b.CreatedAt < currentWeekEnd).ToList();

        int cwEvents = currentWeekEvents.Count;
        int cwUpcoming = currentWeekEvents.Count(e => e.EventDate >= DateTime.Today);
        int cwTickets = currentWeekPaid.Sum(b => b.Quantity);
        decimal cwRevenue = currentWeekPaid.Sum(b => b.TotalAmount);
        int cwAttendees = currentWeekPaid.Select(b => b.UserId).Distinct().Count();

        // Last week
        List<Event> lastWeekEvents = allEvents.Where(e => e.CreatedAt >= lastWeekStart && e.CreatedAt < currentWeekStart).ToList();
        List<Booking> lastWeekPaid = allPaidBookings.Where(b => b.CreatedAt >= lastWeekStart && b.CreatedAt < currentWeekStart).ToList();

        int lwEvents = lastWeekEvents.Count;
        int lwUpcoming = lastWeekEvents.Count(e => e.EventDate >= DateTime.Today);
        int lwTickets = lastWeekPaid.Sum(b => b.Quantity);
        decimal lwRevenue = lastWeekPaid.Sum(b => b.TotalAmount);
        int lwAttendees = lastWeekPaid.Select(b => b.UserId).Distinct().Count();

        List<MonthlyRevenueDto> monthlyRevenue = [];

        List<CategoryEventCountDto> eventsByCategory = allEvents
            .Where(e => e.Category != null)
            .GroupBy(e => e.Category!.Name)
            .Select(g => new CategoryEventCountDto
            {
                CategoryName = g.Key,
                Count = g.Count(),
            })
            .ToList();

        // Top booked events
        var bookingStats = allPaidBookings
            .GroupBy(b => b.EventId)
            .Select(g => new
            {
                EventId = g.Key,
                TotalBookings = g.Sum(b => b.Quantity),
                RevenueGenerated = g.Sum(b => b.TotalAmount),
            })
            .OrderByDescending(x => x.TotalBookings)
            .Take(10)
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

        List<DashboardRecentAttendeeDto> recentAttendees = allPaidBookings
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
                EventsChange = GrowthPercent(cwEvents, lwEvents),
                UpcomingEventsChange = GrowthPercent(cwUpcoming, lwUpcoming),
                TicketsSoldChange = GrowthPercent(cwTickets, lwTickets),
                RevenueChange = GrowthPercent(cwRevenue, lwRevenue),
                AttendeesChange = GrowthPercent(cwAttendees, lwAttendees),
            },
            EventsByCategory = eventsByCategory,
            MonthlyRevenue = monthlyRevenue,
            RecentAttendees = recentAttendees,
            TopBookedEvents = topBookedEvents,
        };
    }

    public async Task<OrganizerDashboardDto> GetAdminDashboardDataAsync(int? organizerId = null)
    {
        var (currentWeekStart, currentWeekEnd) = GetCurrentWeekBounds();
        DateTime lastWeekStart = currentWeekStart.AddDays(-7);

        List<Event> allEvents = organizerId.HasValue
            ? await _eventRepository.GetEventsByOrganizerIdAsync(organizerId.Value)
            : await _eventRepository.GetAllEventsWithDetailsAsync();

        List<BookingProjection> allPaidBookings;
        if (organizerId.HasValue)
        {
            allPaidBookings = (await _eventRepository.GetBookingsByOrganizerIdAsync(organizerId.Value))
                .Where(b => b.PaymentStatus == PaymentStatus.Paid)
                .Select(b => new BookingProjection
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    EventId = b.EventId,
                    Quantity = b.Quantity,
                    TotalAmount = b.TotalAmount,
                    PaymentStatus = b.PaymentStatus,
                    CreatedAt = b.CreatedAt,
                    IsDeleted = b.IsDeleted,
                }).ToList();
        }
        else
        {
            allPaidBookings = await _bookingRepository.GetAllBookingsAsync();
        }

        int totalEvents = allEvents.Count;
        int upcomingEvents = allEvents.Count(e => e.EventDate >= DateTime.Today);
        int totalTicketsSold = allPaidBookings.Sum(b => b.Quantity);
        decimal totalRevenue = allPaidBookings.Sum(b => b.TotalAmount);
        int totalAttendees = allPaidBookings.Select(b => b.UserId).Distinct().Count();

        // Current week
        List<Event> currentWeekEvents = allEvents.Where(e => e.CreatedAt >= currentWeekStart && e.CreatedAt < currentWeekEnd).ToList();
        List<BookingProjection> currentWeekPaid = allPaidBookings.Where(b => b.CreatedAt >= currentWeekStart && b.CreatedAt < currentWeekEnd).ToList();

        int cwEvents = currentWeekEvents.Count;
        int cwUpcoming = currentWeekEvents.Count(e => e.EventDate >= DateTime.Today);
        int cwTickets = currentWeekPaid.Sum(b => b.Quantity);
        decimal cwRevenue = currentWeekPaid.Sum(b => b.TotalAmount);
        int cwAttendees = currentWeekPaid.Select(b => b.UserId).Distinct().Count();

        // Last week
        List<Event> lastWeekEvents = allEvents.Where(e => e.CreatedAt >= lastWeekStart && e.CreatedAt < currentWeekStart).ToList();
        List<BookingProjection> lastWeekPaid = allPaidBookings.Where(b => b.CreatedAt >= lastWeekStart && b.CreatedAt < currentWeekStart).ToList();

        int lwEvents = lastWeekEvents.Count;
        int lwUpcoming = lastWeekEvents.Count(e => e.EventDate >= DateTime.Today);
        int lwTickets = lastWeekPaid.Sum(b => b.Quantity);
        decimal lwRevenue = lastWeekPaid.Sum(b => b.TotalAmount);
        int lwAttendees = lastWeekPaid.Select(b => b.UserId).Distinct().Count();

        List<MonthlyRevenueDto> monthlyRevenue = [];

        List<CategoryEventCountDto> eventsByCategory = allEvents
            .Where(e => e.Category != null)
            .GroupBy(e => e.Category!.Name)
            .Select(g => new CategoryEventCountDto
            {
                CategoryName = g.Key,
                Count = g.Count(),
            })
            .ToList();

        List<TopBookedEventDto> topBookedEvents = allPaidBookings
            .GroupBy(b => b.EventId)
            .Select(g => new
            {
                EventId = g.Key,
                TotalBookings = g.Sum(b => b.Quantity),
                RevenueGenerated = g.Sum(b => b.TotalAmount),
            })
            .OrderByDescending(x => x.TotalBookings)
            .Take(10)
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
                EventsChange = GrowthPercent(cwEvents, lwEvents),
                UpcomingEventsChange = GrowthPercent(cwUpcoming, lwUpcoming),
                TicketsSoldChange = GrowthPercent(cwTickets, lwTickets),
                RevenueChange = GrowthPercent(cwRevenue, lwRevenue),
                AttendeesChange = GrowthPercent(cwAttendees, lwAttendees),
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
        List<BookingProjection> paidBookings = bookings
            .Where(b => b.PaymentStatus == PaymentStatus.Paid)
            .Select(b => new BookingProjection
            {
                Id = b.Id,
                UserId = b.UserId,
                EventId = b.EventId,
                Quantity = b.Quantity,
                TotalAmount = b.TotalAmount,
                PaymentStatus = b.PaymentStatus,
                CreatedAt = b.CreatedAt,
                IsDeleted = b.IsDeleted,
            }).ToList();
        return WeekMonthYearWiseRevenue(paidBookings, period);
    }

    public async Task<List<MonthlyRevenueDto>> GetAdminRevenueTrendAsync(string? period = "year", int? organizerId = null)
    {
        List<BookingProjection> paidBookings;
        if (organizerId.HasValue)
        {
            paidBookings = (await _eventRepository.GetBookingsByOrganizerIdAsync(organizerId.Value))
                .Where(b => b.PaymentStatus == PaymentStatus.Paid)
                .Select(b => new BookingProjection
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    EventId = b.EventId,
                    Quantity = b.Quantity,
                    TotalAmount = b.TotalAmount,
                    PaymentStatus = b.PaymentStatus,
                    CreatedAt = b.CreatedAt,
                    IsDeleted = b.IsDeleted,
                }).ToList();
        }
        else
        {
            paidBookings = await _bookingRepository.GetAllBookingsAsync();
        }

        return WeekMonthYearWiseRevenue(paidBookings, period);
    }

    private static List<MonthlyRevenueDto> WeekMonthYearWiseRevenue(List<BookingProjection> paidBookings, string? period)
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
                .OrderBy(m => m.Key)
                .Select(g => new MonthlyRevenueDto
                {
                    Month = $"{now:MMM} {g.Key}",
                    Revenue = g.Sum(b => b.TotalAmount),
                    Bookings = g.Sum(b => b.Quantity),
                })
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