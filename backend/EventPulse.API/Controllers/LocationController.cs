using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.Location;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Route("api/locations")]
[ApiController]
public class LocationController : BaseHelper
{
    private readonly ILocationService _locationService;

    public LocationController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries()
    {
        List<CountryDto> result = await _locationService.GetAllCountriesAsync();
        return SuccessResponse(result);
    }

    [HttpGet("countries/{countryId}/states")]
    public async Task<IActionResult> GetStates(int countryId)
    {
        List<StateDto> result = await _locationService.GetStatesByCountryIdAsync(countryId);
        return SuccessResponse(result);
    }

    [HttpGet("states/{stateId}/cities")]
    public async Task<IActionResult> GetCities(int stateId)
    {
        List<CityDto> result = await _locationService.GetCitiesByStateIdAsync(stateId);
        return SuccessResponse(result);
    }

    [HttpGet("cities/{cityId}")]
    public async Task<IActionResult> GetCity(int cityId)
    {
        CityDto? result = await _locationService.GetCityByIdAsync(cityId);
        if (result is null) return NotFoundResponse("City not found.");
        return SuccessResponse(result);
    }
}
