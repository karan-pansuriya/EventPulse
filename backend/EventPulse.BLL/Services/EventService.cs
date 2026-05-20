using Microsoft.EntityFrameworkCore;
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

    public EventService(
        EventPulseDbContext context,
        IGenericRepository<Event> eventRepo,
        IGenericRepository<EventPoster> posterRepo,
        IUnitOfWork unitOfWork,
        IImageService imageService)
    {
        _context = context;
        _eventRepo = eventRepo;
        _posterRepo = posterRepo;
        _unitOfWork = unitOfWork;
        _imageService = imageService;
    }

    public async Task<EventResponse> GetByIdAsync(int id, int? userId = null, string? userRole = null)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Include(e => e.Posters)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted)
            ?? throw new NotFoundException("Event not found.");

        if (userRole == "Organizer" && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to view this event.");

        return MapToResponse(eventEntity);
    }

    public async Task<PagedResult<EventListResponse>> GetPagedAsync(PageRequest pageRequest)
    {
        SetPageDefaults(pageRequest);

        return await _eventRepo.GetPagedAsync(
            null,
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

    public async Task<PagedResult<EventListResponse>> GetMyEventsAsync(int organizerId, PageRequest pageRequest)
    {
        SetPageDefaults(pageRequest);

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

        var venueId = await ResolveVenueIdAsync(dto.VenueName, dto.VenueAddress, dto.VenueCity, dto.VenueState, dto.VenueCountry);

        var eventEntity = new Event
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
            foreach (var (imageBytes, fileName) in posterImages)
            {
                var relativePath = await _imageService.SaveImageAsync(imageBytes, fileName, "event_poster");
                var poster = new EventPoster
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

        var eventEntity = await _eventRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Event not found.");

        if (userRole != "Admin" && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to update this event.");

        var venueId = await ResolveVenueIdAsync(dto.VenueName, dto.VenueAddress, dto.VenueCity, dto.VenueState, dto.VenueCountry);

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
            var existingPosters = await _context.EventPosters
                .Where(p => p.EventId == id && !p.IsDeleted)
                .ToListAsync();

            foreach (var ep in existingPosters)
            {
                _imageService.DeleteImage(ep.PosterUrl);
                _posterRepo.Delete(ep);
            }

            foreach (var (imageBytes, fileName) in posterImages)
            {
                var relativePath = await _imageService.SaveImageAsync(imageBytes, fileName, "event_poster");
                var poster = new EventPoster
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
        var eventEntity = await _eventRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Event not found.");

        if (userRole != "Admin" && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to delete this event.");

        var posters = await _context.EventPosters
            .Where(p => p.EventId == id && !p.IsDeleted)
            .ToListAsync();

        foreach (var poster in posters)
        {
            _imageService.DeleteImage(poster.PosterUrl);
            _posterRepo.Delete(poster);
        }

        _eventRepo.Delete(eventEntity);
        await _unitOfWork.SaveAsync();
    }

    private static EventResponse MapToResponse(Event eventEntity)
    {
        return new EventResponse
        {
            Id = eventEntity.Id,
            OrganizerId = eventEntity.OrganizerId,
            OrganizerName = eventEntity.Organizer?.Name,
            CategoryId = eventEntity.CategoryId,
            CategoryName = eventEntity.Category?.Name,
            VenueId = eventEntity.VenueId,
            VenueName = eventEntity.Venue?.Name,
            VenueAddress = eventEntity.Venue?.Address,
            VenueCity = eventEntity.Venue?.City,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            Genre = eventEntity.Genre,
            AgeRestriction = eventEntity.AgeRestriction,
            Performers = eventEntity.Performers,
            DurationMins = eventEntity.DurationMins,
            EventDate = eventEntity.EventDate,
            StartTime = eventEntity.StartTime,
            Price = eventEntity.Price,
            TotalSeats = eventEntity.TotalSeats,
            IsVerified = eventEntity.IsVerified,
            IsActive = eventEntity.IsActive,
            PosterUrl = eventEntity.Posters.Select(p => p.PosterUrl).FirstOrDefault(),
        PosterUrls = eventEntity.Posters.Select(p => p.PosterUrl).ToList(),
            CreatedAt = eventEntity.CreatedAt,
            UpdatedAt = eventEntity.UpdatedAt,
        };
    }

    private async Task<int?> ResolveVenueIdAsync(string? name, string? address, string? city, string? state, string? country)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var existing = await _context.Venues
            .FirstOrDefaultAsync(v => v.Name.ToLower() == name.ToLower() && !v.IsDeleted);

        if (existing != null)
        {
            existing.Address = address ?? existing.Address;
            existing.City = city ?? existing.City;
            existing.State = state ?? existing.State;
            existing.Country = country ?? existing.Country;
            return existing.Id;
        }

        var venue = new Venue
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

    private static void SetPageDefaults(PageRequest pageRequest)
    {
        if (pageRequest.PageNumber < 1) pageRequest.PageNumber = 1;
        if (pageRequest.PageSize < 1) pageRequest.PageSize = 10;
    }
}
