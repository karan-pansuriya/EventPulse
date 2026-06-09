using EventPulse.Common.Models;
using EventPulse.DAL.Entities;

namespace EventPulse.DAL.Repositories.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetEventWithDetailsAsync(int id);

    Task<(List<Event> Items, int TotalCount)> GetCustomerPagedEventsAsync(EventFilterRequest filter);

    Task<Venue?> ResolveVenueAsync(string? name, string? address, int? cityId);

    Task<List<EventPoster>> GetActivePostersByEventIdAsync(int eventId);
    Task<List<Booking>> GetBookingsByOrganizerIdAsync(int organizerId);

    Task<(List<Booking> Items, int TotalCount)> GetPagedBookingsByOrganizerIdAsync(int organizerId, int pageNumber, int pageSize);
    Task<List<Event>> GetEventsByOrganizerIdAsync(int organizerId);

    Task<Event?> GetEventByTitleDateVenueAsync(string title, DateTime eventDate, string venueName);

    Task<int> GetBookingCountByEventIdAsync(int eventId);

    Task<List<Event>> GetAllEventsWithDetailsAsync();
}
