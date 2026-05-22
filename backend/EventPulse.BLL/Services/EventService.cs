using Microsoft.EntityFrameworkCore;
using System.Reflection;
using AutoMapper;
using EventPulse.BLL.DTOs.Event;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Context;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.BLL.Services;

public class EventService : IEventService
{
    private const int MaxPosterImages = 10;

    private readonly EventPulseDbContext _context;
    private readonly IGenericRepository<Event> _eventRepo;
    private readonly IGenericRepository<EventPoster> _posterRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;
    private readonly IMapper _mapper;

    public EventService(
        EventPulseDbContext context,
        IGenericRepository<Event> eventRepo,
        IGenericRepository<EventPoster> posterRepo,
        IUnitOfWork unitOfWork,
        IImageService imageService,
        IMapper mapper)
    {
        _context = context;
        _eventRepo = eventRepo;
        _posterRepo = posterRepo;
        _unitOfWork = unitOfWork;
        _imageService = imageService;
        _mapper = mapper;
    }

    public async Task<EventResponse> GetByIdAsync(int id, int? userId = null, string? userRole = null)
    {
        Event eventEntity = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Include(e => e.Posters)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted)
            ?? throw new NotFoundException("Event not found.");

        if (userRole == "Organizer" && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to view this event.");

        return _mapper.Map<EventResponse>(eventEntity);
    }

    public async Task<PagedResult<EventListResponse>> GetPagedAsync(EventFilterRequest filter)
    {
        IQueryable<Event> query = _context.Events.Where(e => !e.IsDeleted);

        if (filter.DateFrom.HasValue)
            query = query.Where(e => e.EventDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(e => e.EventDate <= filter.DateTo.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(e => e.CategoryId == filter.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(e => e.Venue != null && e.Venue.City.ToLower().Contains(filter.City.ToLower()));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            string search = filter.Search.ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(search) ||
                (e.Description != null && e.Description.ToLower().Contains(search)) ||
                (e.Performers != null && e.Performers.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            PropertyInfo? property = typeof(Event).GetProperty(filter.SortBy);
            if (property != null)
            {
                query = filter.SortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(e => EF.Property<object>(e, filter.SortBy))
                    : query.OrderBy(e => EF.Property<object>(e, filter.SortBy));
            }
        }

        int totalCount = await query.CountAsync();

        List<EventListResponse> items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(e => new EventListResponse
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
            })
            .ToListAsync();

        return new PagedResult<EventListResponse>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResult<EventListResponse>> GetMyEventsAsync(int organizerId, PageRequest pageRequest)
    {
        return await _eventRepo.GetPagedAsync(
            e => e.OrganizerId == organizerId,
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

    public async Task<EventResponse> CreateAsync(int organizerId, CreateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages)
    {
        if (posterImages?.Count > MaxPosterImages)
            throw new BadRequestException($"Maximum {MaxPosterImages} poster images allowed.");

        int? venueId = await ResolveVenueIdAsync(dto.VenueName, dto.VenueAddress, dto.VenueCity, dto.VenueState, dto.VenueCountry);

        Event eventEntity = new Event
        {
            OrganizerId = organizerId,
            CategoryId = dto.CategoryId,
            VenueId = venueId,
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

    public async Task<EventResponse> UpdateAsync(int id, int userId, string userRole, UpdateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages)
    {
        if (posterImages?.Count > MaxPosterImages)
            throw new BadRequestException($"Maximum {MaxPosterImages} poster images allowed.");

        Event? eventEntity = await _eventRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Event not found.");

        if (userRole != "Admin" && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to update this event.");

        int? venueId = await ResolveVenueIdAsync(dto.VenueName, dto.VenueAddress, dto.VenueCity, dto.VenueState, dto.VenueCountry);

        eventEntity.CategoryId = dto.CategoryId;
        eventEntity.VenueId = venueId;
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

        if (posterImages is { Count: > 0 })
        {
            List<EventPoster> existingPosters = await _context.EventPosters
                .Where(p => p.EventId == id && !p.IsDeleted)
                .ToListAsync();

            foreach (EventPoster ep in existingPosters)
            {
                _imageService.DeleteImage(ep.PosterUrl);
                _posterRepo.Delete(ep);
            }

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

    public async Task DeleteAsync(int id, int userId, string userRole)
    {
        Event? eventEntity = await _eventRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Event not found.");

        if (userRole != "Admin" && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to delete this event.");

        List<EventPoster> posters = await _context.EventPosters
            .Where(p => p.EventId == id && !p.IsDeleted)
            .ToListAsync();

        foreach (EventPoster poster in posters)
        {
            _imageService.DeleteImage(poster.PosterUrl);
            _posterRepo.Delete(poster);
        }

        _eventRepo.Delete(eventEntity);
        await _unitOfWork.SaveAsync();
    }

    private async Task<int?> ResolveVenueIdAsync(string? name, string? address, string? city, string? state, string? country)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        Venue? existing = await _context.Venues
            .FirstOrDefaultAsync(v => v.Name.ToLower() == name.ToLower() && !v.IsDeleted);

        if (existing != null)
        {
            existing.Address = address ?? existing.Address;
            existing.City = city ?? existing.City;
            existing.State = state ?? existing.State;
            existing.Country = country ?? existing.Country;
            return existing.Id;
        }

        Venue venue = new Venue
        {
            Name = name,
            Address = address ?? string.Empty,
            City = city ?? string.Empty,
            State = state,
            Country = country ?? string.Empty,
            IsActive = true,
        };

        _context.Venues.Add(venue);
        await _unitOfWork.SaveAsync();

        return venue.Id;
    }

}
