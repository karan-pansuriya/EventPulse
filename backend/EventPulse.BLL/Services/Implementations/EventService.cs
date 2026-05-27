using AutoMapper;
using EventPulse.BLL.Common;
using EventPulse.BLL.DTOs.Event;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.BLL.Services;

public class EventService : IEventService
{
    private const int MaxPosterImages = 10;

    private readonly IGenericRepository<Event> _eventRepo;
    private readonly IGenericRepository<EventPoster> _posterRepo;
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;
    private readonly IMapper _mapper;

    public EventService(
        IGenericRepository<Event> eventRepo,
        IGenericRepository<EventPoster> posterRepo,
        IEventRepository eventRepository,
        IUnitOfWork unitOfWork,
        IImageService imageService,
        IMapper mapper)
    {
        _eventRepo = eventRepo;
        _posterRepo = posterRepo;
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
        _imageService = imageService;
        _mapper = mapper;
    }

    public async Task<EventResponse> GetByIdAsync(int id, int? userId = null, int? userRoleId = null)
    {
        Event eventEntity = await _eventRepository.GetEventWithDetailsAsync(id)
            ?? throw new NotFoundException("Event not found.");

        if (userRoleId == RoleId.Organizer && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to view this event.");

        return _mapper.Map<EventResponse>(eventEntity);
    }

    public async Task<PagedResult<EventListResponse>> GetPagedAsync(EventFilterRequest filter)
    {
        (List<Event> items, int totalCount) = await _eventRepository.GetPagedEventsAsync(filter);

        List<EventListResponse> responseItems = items.Select(e => new EventListResponse
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
        }).ToList();

        return new PagedResult<EventListResponse>
        {
            Items = responseItems,
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

        Venue? venue = await _eventRepository.ResolveVenueAsync(dto.VenueName, dto.VenueAddress, dto.VenueCity, dto.VenueState, dto.VenueCountry);
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

    public async Task<EventResponse> UpdateAsync(int id, int userId, int userRoleId, UpdateEventDto dto, List<(byte[] ImageBytes, string FileName)>? posterImages)
    {
        if (posterImages?.Count > MaxPosterImages)
            throw new BadRequestException($"Maximum {MaxPosterImages} poster images allowed.");

        Event? eventEntity = await _eventRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Event not found.");

        if (userRoleId != RoleId.Admin && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to update this event.");

        Venue? venue = await _eventRepository.ResolveVenueAsync(dto.VenueName, dto.VenueAddress, dto.VenueCity, dto.VenueState, dto.VenueCountry);
        if (venue != null && venue.Id == 0)
        {
            await _unitOfWork.SaveAsync();
        }

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

        if (posterImages is { Count: > 0 })
        {
            List<EventPoster> existingPosters = await _eventRepository.GetActivePostersByEventIdAsync(id);

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

    public async Task DeleteAsync(int id, int userId, int userRoleId)
    {
        Event? eventEntity = await _eventRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Event not found.");

        if (userRoleId != RoleId.Admin && eventEntity.OrganizerId != userId)
            throw new ForbiddenException("You are not authorized to delete this event.");

        List<EventPoster> posters = await _eventRepository.GetActivePostersByEventIdAsync(id);

        foreach (EventPoster poster in posters)
        {
            _imageService.DeleteImage(poster.PosterUrl);
            _posterRepo.Delete(poster);
        }

        _eventRepo.Delete(eventEntity);
        await _unitOfWork.SaveAsync();
    }

}
