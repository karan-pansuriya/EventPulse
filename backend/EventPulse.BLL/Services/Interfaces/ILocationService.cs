using EventPulse.BLL.DTOs.Location;

namespace EventPulse.BLL.Interfaces;

public interface ILocationService
{
    Task<List<CountryDto>> GetAllCountriesAsync();
    Task<List<StateDto>> GetStatesByCountryIdAsync(int countryId);
    Task<List<CityDto>> GetCitiesByStateIdAsync(int stateId);
    Task<CityDto?> GetCityByIdAsync(int cityId);
}
