using System.Reflection;
using EventPulse.Common.Models;
using EventPulse.DAL.Context;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.DAL.Repositories.Implementations;

public class EventRepository(EventPulseDbContext context) : IEventRepository
{
    private readonly EventPulseDbContext _context = context;

    public async Task<Event?> GetEventWithDetailsAsync(int id)
    {
        return await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Venue).ThenInclude(v => v!.City).ThenInclude(c => c!.State).ThenInclude(s => s!.Country)
            .Include(e => e.Posters)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<(List<Event> Items, int TotalCount)> GetCustomerPagedEventsAsync(EventFilterRequest filter)
    {
        IQueryable<Event> query = _context.Events
            .AsNoTracking()
            .Where(e => e.IsVerified && e.EventDate >= DateTime.Today);

        if (filter.DateFrom.HasValue)
            query = query.Where(e => e.EventDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(e => e.EventDate <= filter.DateTo.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(e => e.CategoryId == filter.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(filter.City))
            query = query.Where(e => e.Venue != null && e.Venue.City != null && EF.Functions.ILike(e.Venue.City.Name, $"%{filter.City}%"));

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(e => EF.Functions.ILike(e.Title, $"%{filter.Search}%"));
        }

        string sortBy = string.IsNullOrWhiteSpace(filter.SortBy) ? "EventDate" : filter.SortBy;
        PropertyInfo? property = typeof(Event).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (property != null)
        {
            query = filter.SortDirection?.ToLower() == "desc"
                ? query.OrderByDescending(e => EF.Property<object>(e, property.Name))
                : query.OrderBy(e => EF.Property<object>(e, property.Name));
        }
        else
        {
            query = query.OrderBy(e => e.EventDate);
        }

        int totalCount = await query.CountAsync();

        List<Event> items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(e => e.Category)
            .Include(e => e.Venue).ThenInclude(v => v!.City).ThenInclude(c => c!.State).ThenInclude(s => s!.Country)
            .Include(e => e.Posters)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Venue?> ResolveVenueAsync(string? name, string? address, int? cityId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        Venue? existing = await _context.Venues
            .Include(v => v.City).ThenInclude(c => c!.State).ThenInclude(s => s!.Country)
            .FirstOrDefaultAsync(v => EF.Functions.ILike(v.Name, name));

        if (existing != null)
        {
            existing.Address = address ?? existing.Address;
            if (cityId.HasValue && cityId.Value > 0)
                existing.CityId = cityId.Value;
            return existing;
        }

        City? city = cityId.HasValue
            ? await _context.Cities
                .Include(c => c.State).ThenInclude(s => s!.Country)
                .FirstOrDefaultAsync(c => c.Id == cityId.Value)
            : null;

        if (city is null)
            throw new InvalidOperationException("Selected city not found.");

        Venue venue = new Venue
        {
            Name = name,
            Address = address ?? string.Empty,
            CityId = city.Id,
        };

        _context.Venues.Add(venue);
        return venue;
    }

    public async Task<List<Booking>> GetBookingsByOrganizerIdAsync(int organizerId)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.Event!.OrganizerId == organizerId)
            .Include(b => b.User)
            .Include(b => b.Event).ThenInclude(e => e!.Category)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<(List<Booking> Items, int TotalCount)> GetPagedBookingsByOrganizerIdAsync(int organizerId, int pageNumber, int pageSize)
    {
        IQueryable<Booking> query = _context.Bookings
            .AsNoTracking()
            .Where(b => b.Event!.OrganizerId == organizerId);

        int totalCount = await query.CountAsync();

        List<Booking> items = await query
            .Include(b => b.User)
            .Include(b => b.Event).ThenInclude(e => e!.Category)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Event>> GetEventsByOrganizerIdAsync(int organizerId)
    {
        return await _context.Events
            .AsNoTracking()
            .Where(e => e.OrganizerId == organizerId)
            .Include(e => e.Category)
            .Include(e => e.Posters)
            .Include(e => e.Organizer)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<EventPoster>> GetActivePostersByEventIdAsync(int eventId)
    {
        return await _context.EventPosters
            .AsNoTracking()
            .Where(p => p.EventId == eventId)
            .ToListAsync();
    }

    public async Task<Event?> GetEventByTitleDateVenueAsync( DateTime eventDate, string venueName)
    {
        return await _context.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e =>
                e.EventDate == eventDate &&
                e.Venue != null && e.Venue.Name.ToLower() == venueName.ToLower());
    }

    public async Task<int> GetBookingCountByEventIdAsync(int eventId)
    {
        return await _context.Bookings
            .CountAsync(b => b.EventId == eventId && b.PaymentStatus == Enums.PaymentStatus.Paid);
    }

    public async Task<List<Event>> GetAllEventsWithDetailsAsync()
    {
        return await _context.Events
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.Venue).ThenInclude(v => v!.City).ThenInclude(c => c!.State).ThenInclude(s => s!.Country)
            .Include(e => e.Posters)
            .Include(e => e.Organizer)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }
}
