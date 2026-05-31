using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.User;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Authorize]
[Route("api/profile")]
public class ProfileController : BaseHelper
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        UserDto result = await _profileService.GetProfileAsync();
        return SuccessResponse(result);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
    {
        UserDto result = await _profileService.UpdateProfileAsync(request);
        return SuccessResponse(result);
    }
}
