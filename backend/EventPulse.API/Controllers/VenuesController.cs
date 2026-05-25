using EventPulse.Bll.Helpers;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Route("api/venues")]
public class VenuesController : BaseController
{
    private readonly IVenueService _venueService;

    public VenuesController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var cities = await _venueService.GetCitiesAsync();

        return SuccessResponse(cities);
    }
}