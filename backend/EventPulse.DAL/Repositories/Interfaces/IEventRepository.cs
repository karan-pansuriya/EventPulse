using EventPulse.Common.Models;
using EventPulse.DAL.Entities;

namespace EventPulse.DAL.Repositories.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetEventWithDetailsAsync(int id);

    Task<(List<Event> Items, int TotalCount)> GetPagedEventsAsync(EventFilterRequest filter);

    Task<Venue?> ResolveVenueAsync(string? name, string? address, int? cityId);

    Task<List<EventPoster>> GetActivePostersByEventIdAsync(int eventId);
    Task<List<Booking>> GetBookingsByOrganizerIdAsync(int organizerId);
}
