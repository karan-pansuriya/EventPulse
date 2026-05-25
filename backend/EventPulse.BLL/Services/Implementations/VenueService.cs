using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;

namespace EventPulse.BLL.Services;

public class VenueService : IVenueService
{
    private readonly IGenericService<Venue> _venueService;

    public VenueService(IGenericService<Venue> venueService)
    {
        _venueService = venueService;
    }

    public async Task<List<string>> GetCitiesAsync()
    {
        var venues = await _venueService.GetAllAsync();

        return venues
            .Select(v => v.City)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c)
            .ToList();
    }
}