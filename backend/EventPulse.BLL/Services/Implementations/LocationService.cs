using EventPulse.BLL.DTOs.Location;
using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Context;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.BLL.Services;

public class LocationService : ILocationService
{
    private readonly EventPulseDbContext _context;

    public LocationService(EventPulseDbContext context)
    {
        _context = context;
    }

    public async Task<List<CountryDto>> GetAllCountriesAsync()
    {
        return await _context.Countries
            .OrderBy(c => c.Name)
            .Select(c => new CountryDto { Id = c.Id, Name = c.Name })
            .ToListAsync();
    }

    public async Task<List<StateDto>> GetStatesByCountryIdAsync(int countryId)
    {
        return await _context.States
            .Where(s => s.CountryId == countryId)
            .OrderBy(s => s.Name)
            .Select(s => new StateDto { Id = s.Id, Name = s.Name, CountryId = s.CountryId })
            .ToListAsync();
    }

    public async Task<List<CityDto>> GetCitiesByStateIdAsync(int stateId)
    {
        return await _context.Cities
            .Where(c => c.StateId == stateId)
            .OrderBy(c => c.Name)
            .Select(c => new CityDto { Id = c.Id, Name = c.Name, StateId = c.StateId, CountryId = c.State.CountryId })
            .ToListAsync();
    }

    public async Task<CityDto?> GetCityByIdAsync(int cityId)
    {
        return await _context.Cities
            .Where(c => c.Id == cityId)
            .Select(c => new CityDto { Id = c.Id, Name = c.Name, StateId = c.StateId, CountryId = c.State.CountryId })
            .FirstOrDefaultAsync();
    }
}
