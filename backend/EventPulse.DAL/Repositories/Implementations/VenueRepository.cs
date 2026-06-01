using EventPulse.DAL.Context;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.DAL.Repositories.Implementations;

public class VenueRepository(EventPulseDbContext context) : IVenueRepository
{
    private readonly EventPulseDbContext _context = context;

    public async Task<List<string>> GetCityNamesAsync()
    {
        return await _context.Venues
            .Where(v => v.City != null)
            .Select(v => v.City!.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync();
    }
}
