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
            .Include(e => e.Category)
            .Include(e => e.Venue)
            .Include(e => e.Posters)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<(List<Event> Items, int TotalCount)> GetPagedEventsAsync(EventFilterRequest filter)
    {
        IQueryable<Event> query = _context.Events
            .Where(e => !e.IsDeleted && e.IsVerified && e.EventDate >= DateTime.Today);

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
            .Include(e => e.Venue)
            .Include(e => e.Posters)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Venue?> ResolveVenueAsync(string? name, string? address, string? city, string? state, string? country)
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
            return existing;
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
        return venue;
    }

    public async Task<List<EventPoster>> GetActivePostersByEventIdAsync(int eventId)
    {
        return await _context.EventPosters
            .Where(p => p.EventId == eventId && !p.IsDeleted)
            .ToListAsync();
    }
}
