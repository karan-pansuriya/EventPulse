using System.Security.Claims;
using AutoMapper;
using EventPulse.BLL.Common;
using Microsoft.AspNetCore.Http;
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

public class EventService : IEventService
{
    private const int MaxPosterImages = 5;
    private const long MaxPosterFileSize = 5 * 1024 * 1024; // 5 MB

    private readonly IGenericRepository<Event> _eventRepo;
    private readonly IGenericRepository<EventPoster> _posterRepo;
    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EventService(
        IGenericRepository<Event> eventRepo,
        IGenericRepository<EventPoster> posterRepo,
        IEventRepository eventRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork,
        IImageService imageService,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        _eventRepo = eventRepo;
        _posterRepo = posterRepo;
        _eventRepository = eventRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
        _imageService = imageService;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    private int GetUserId()
    {
        Claim? claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? _httpContextAccessor.HttpContext?.User.FindFirst("sub");
        if (claim == null || !int.TryParse(claim.Value, out int id))
            throw new UnauthorizedAccessException("User ID not found in token.");
        return id;
    }

    private int? GetActiveRoleId()
    {
        string? value = _httpContextAccessor.HttpContext?.User.FindFirst("active_role_id")?.Value;
        if (int.TryParse(value, out int roleId) && roleId > 0)
            return roleId;
        return null;
    }

    public async Task<EventResponse> GetByIdAsync(int id)
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

    public async Task<PagedResult<EventListResponse>> GetPagedAsync(EventFilterRequest filter)
    {
        (List<Event> items, int totalCount) = await _eventRepository.GetPagedEventsAsync(filter);

        List<EventListResponse> responseItems = _mapper.Map<List<EventListResponse>>(items);

        return new PagedResult<EventListResponse>
        {
            Items = responseItems,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<EventListResponse>> GetMyEventsAsync(PageRequest pageRequest)
    {
        int organizerId = GetUserId();
        return await _eventRepo.GetPagedAsync(
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
    }

    public async Task<PagedResult<EventListResponse>> GetAllEventsAsync(PageRequest pageRequest)
    {
        return await _eventRepo.GetPagedAsync(
            e => !e.IsDeleted,
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
    }

    public async Task ToggleVerificationAsync(int id)
    {
        Event? eventEntity = await _eventRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Event not found.");

        eventEntity.IsVerified = !eventEntity.IsVerified;
        _eventRepo.Update(eventEntity);
        await _unitOfWork.SaveAsync();
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

    public async Task<EventResponse> CreateAsync(CreateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages)
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

        return await GetByIdAsync(eventEntity.Id);
    }

    public async Task<EventResponse> UpdateAsync(int id, UpdateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages)
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

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id)
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
    }

    public async Task<OrganizerDashboardDto> GetDashboardDataAsync(string? period = "year")
    {
        int organizerId = GetUserId();

        List<Event> events = await _eventRepository.GetEventsByOrganizerIdAsync(organizerId);
        List<Booking> bookings = await _eventRepository.GetBookingsByOrganizerIdAsync(organizerId);

        int totalEvents = events.Count;
        int upcomingEvents = events.Count(e => e.EventDate >= DateTime.Today);

        List<Booking> paidBookings = bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).ToList();
        int totalTicketsSold = paidBookings.Sum(b => b.Quantity);
        decimal totalRevenue = paidBookings.Sum(b => b.TotalAmount);
        int totalAttendees = paidBookings.Select(b => b.UserId).Distinct().Count();

        // Weekly comparison
        DateTime weekAgo = DateTime.Today.AddDays(-7);
        DateTime twoWeeksAgo = DateTime.Today.AddDays(-14);

        int thisWeekEvents = events.Count(e => e.CreatedAt >= weekAgo);
        int lastWeekEvents = events.Count(e => e.CreatedAt >= twoWeeksAgo && e.CreatedAt < weekAgo);

        int thisWeekUpcoming = events.Count(e => e.EventDate >= DateTime.Today && e.CreatedAt >= weekAgo);
        int lastWeekUpcoming = events.Count(e => e.EventDate >= DateTime.Today && e.CreatedAt >= twoWeeksAgo && e.CreatedAt < weekAgo);

        List<Booking> thisWeekBookings = paidBookings.Where(b => b.CreatedAt >= weekAgo).ToList();
        List<Booking> lastWeekBookings = paidBookings.Where(b => b.CreatedAt >= twoWeeksAgo && b.CreatedAt < weekAgo).ToList();

        int lastWeekTicketsSold = lastWeekBookings.Sum(b => b.Quantity);
        int thisWeekTicketsSold = thisWeekBookings.Sum(b => b.Quantity);

        decimal lastWeekRevenueAmt = lastWeekBookings.Sum(b => b.TotalAmount);
        decimal thisWeekRevenueAmt = thisWeekBookings.Sum(b => b.TotalAmount);

        int lastWeekAttendeeCount = lastWeekBookings.Select(b => b.UserId).Distinct().Count();
        int thisWeekAttendeeCount = thisWeekBookings.Select(b => b.UserId).Distinct().Count();

        // Revenue by period
        List<MonthlyRevenueDto> monthlyRevenue;
        string periodKey = period?.ToLower() ?? "year";

        if (periodKey == "week")
        {
            string[] dayNames = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            DateTime weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            monthlyRevenue = paidBookings
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
        else if (periodKey == "month")
        {
            DateTime now = DateTime.Today;
            monthlyRevenue = paidBookings
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
        else
        {
            monthlyRevenue = paidBookings
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

        List<CategoryEventCountDto> eventsByCategory = events
            .Where(e => e.Category != null)
            .GroupBy(e => e.Category!.Name)
            .Select(g => new CategoryEventCountDto
            {
                CategoryName = g.Key,
                Count = g.Count(),
            })
            .ToList();

        // Top booked events
        var bookingStats = paidBookings
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
                Event? ev = events.FirstOrDefault(e => e.Id == bs.EventId);
                if (ev == null) return null;
                TopBookedEventDto dto = _mapper.Map<TopBookedEventDto>(ev);
                dto.TotalBookings = bs.TotalBookings;
                dto.RevenueGenerated = bs.RevenueGenerated;
                return dto;
            })
            .OfType<TopBookedEventDto>()
            .ToList();

        List<DashboardRecentAttendeeDto> recentAttendees = paidBookings
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
                EventsChange = lastWeekEvents > 0 ? (thisWeekEvents - lastWeekEvents) * 100 / lastWeekEvents : (thisWeekEvents > 0 ? 100 : 0),
                UpcomingEventsChange = lastWeekUpcoming > 0 ? (thisWeekUpcoming - lastWeekUpcoming) * 100 / lastWeekUpcoming : (thisWeekUpcoming > 0 ? 100 : 0),
                TicketsSoldChange = lastWeekTicketsSold > 0 ? (thisWeekTicketsSold - lastWeekTicketsSold) * 100 / lastWeekTicketsSold : (thisWeekTicketsSold > 0 ? 100 : 0),
                RevenueChange = lastWeekRevenueAmt > 0 ? (thisWeekRevenueAmt - lastWeekRevenueAmt) * 100m / lastWeekRevenueAmt : (thisWeekRevenueAmt > 0 ? 100m : 0m),
                AttendeesChange = lastWeekAttendeeCount > 0 ? (thisWeekAttendeeCount - lastWeekAttendeeCount) * 100 / lastWeekAttendeeCount : (thisWeekAttendeeCount > 0 ? 100 : 0),
            },
            EventsByCategory = eventsByCategory,
            MonthlyRevenue = monthlyRevenue,
            RecentAttendees = recentAttendees,
            TopBookedEvents = topBookedEvents,
        };
    }

    public async Task<OrganizerDashboardDto> GetAdminDashboardDataAsync(string? period = "year", int? organizerId = null)
    {
        List<Event> events = organizerId.HasValue
            ? await _eventRepository.GetEventsByOrganizerIdAsync(organizerId.Value)
            : await _eventRepository.GetAllEventsWithDetailsAsync();

        List<Booking> bookings = organizerId.HasValue
            ? await _eventRepository.GetBookingsByOrganizerIdAsync(organizerId.Value)
            : await _bookingRepository.GetAllBookingsAsync();

        int totalEvents = events.Count;

        int upcomingEvents = events.Count(e => e.EventDate >= DateTime.Today);

        List<Booking> paidBookings = bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).ToList();
        int totalTicketsSold = paidBookings.Sum(b => b.Quantity);

        decimal totalRevenue = paidBookings.Sum(b => b.TotalAmount);

        int totalAttendees = paidBookings.Select(b => b.UserId).Distinct().Count();

        DateTime weekAgo = DateTime.Today.AddDays(-7);
        DateTime twoWeeksAgo = DateTime.Today.AddDays(-14);

        int thisWeekEvents = events.Count(e => e.CreatedAt >= weekAgo);
        int lastWeekEvents = events.Count(e => e.CreatedAt >= twoWeeksAgo && e.CreatedAt < weekAgo);

        int thisWeekUpcoming = events.Count(e => e.EventDate >= DateTime.Today && e.CreatedAt >= weekAgo);
        int lastWeekUpcoming = events.Count(e => e.EventDate >= DateTime.Today && e.CreatedAt >= twoWeeksAgo && e.CreatedAt < weekAgo);

        List<Booking> thisWeekBookings = paidBookings.Where(b => b.CreatedAt >= weekAgo).ToList();
        List<Booking> lastWeekBookings = paidBookings.Where(b => b.CreatedAt >= twoWeeksAgo && b.CreatedAt < weekAgo).ToList();

        int lastWeekTicketsSold = lastWeekBookings.Sum(b => b.Quantity);
        int thisWeekTicketsSold = thisWeekBookings.Sum(b => b.Quantity);

        decimal lastWeekRevenueAmt = lastWeekBookings.Sum(b => b.TotalAmount);
        decimal thisWeekRevenueAmt = thisWeekBookings.Sum(b => b.TotalAmount);

        int lastWeekAttendeeCount = lastWeekBookings.Select(b => b.UserId).Distinct().Count();
        int thisWeekAttendeeCount = thisWeekBookings.Select(b => b.UserId).Distinct().Count();

        List<MonthlyRevenueDto> monthlyRevenue;
        string periodKey = period?.ToLower() ?? "year";

        if (periodKey == "week")
        {
            string[] dayNames = new[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            DateTime weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            monthlyRevenue = paidBookings
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
        else if (periodKey == "month")
        {
            DateTime now = DateTime.Today;
            monthlyRevenue = paidBookings
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
        else
        {
            monthlyRevenue = paidBookings
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

        List<CategoryEventCountDto> eventsByCategory = events
            .Where(e => e.Category != null)
            .GroupBy(e => e.Category!.Name)
            .Select(g => new CategoryEventCountDto
            {
                CategoryName = g.Key,
                Count = g.Count(),
            })
            .ToList();


        List<TopBookedEventDto> topBookedEvents = paidBookings
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
                Event? ev = events.FirstOrDefault(e => e.Id == bs.EventId);
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
                EventsChange = lastWeekEvents > 0 ? (thisWeekEvents - lastWeekEvents) * 100 / lastWeekEvents : (thisWeekEvents > 0 ? 100 : 0),
                UpcomingEventsChange = lastWeekUpcoming > 0 ? (thisWeekUpcoming - lastWeekUpcoming) * 100 / lastWeekUpcoming : (thisWeekUpcoming > 0 ? 100 : 0),
                TicketsSoldChange = lastWeekTicketsSold > 0 ? (thisWeekTicketsSold - lastWeekTicketsSold) * 100 / lastWeekTicketsSold : (thisWeekTicketsSold > 0 ? 100 : 0),
                RevenueChange = lastWeekRevenueAmt > 0 ? (thisWeekRevenueAmt - lastWeekRevenueAmt) * 100m / lastWeekRevenueAmt : (thisWeekRevenueAmt > 0 ? 100m : 0m),
                AttendeesChange = lastWeekAttendeeCount > 0 ? (thisWeekAttendeeCount - lastWeekAttendeeCount) * 100 / lastWeekAttendeeCount : (thisWeekAttendeeCount > 0 ? 100 : 0),
            },
            TopBookedEvents = topBookedEvents,
            RecentAttendees = [],
            MonthlyRevenue = monthlyRevenue,
            EventsByCategory = eventsByCategory,
        };
    }

}
