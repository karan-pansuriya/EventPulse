using EventPulse.API.Helpers;
using EventPulse.DAL.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.API.Controllers;

[Route("api/venues")]
public class VenuesController : BaseHelper
{
    private readonly EventPulseDbContext _context;

    public VenuesController(EventPulseDbContext context)
    {
        _context = context;
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var cities = await _context.Venues
            .Where(v => v.City != null)
            .Select(v => v.City!.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync();

        return SuccessResponse(cities);
    }
}
