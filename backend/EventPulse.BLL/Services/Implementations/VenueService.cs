using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.BLL.Services;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _venueRepository;

    public VenueService(IVenueRepository venueRepository)
    {
        _venueRepository = venueRepository;
    }

    public async Task<List<string>> GetCityNamesAsync()
    {
        return await _venueRepository.GetCityNamesAsync();
    }
}
