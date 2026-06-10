using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.User;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[Route("api/users")]
public class UsersController : BaseHelper
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] PageRequest pageRequest, [FromQuery] int? roleId = null)
    {
        PagedResult<UserListResponse> result = await _userService.GetPagedUsersAsync(pageRequest, roleId);
        return SuccessResponse(result);
    }

    [HttpGet("organizers")]
    public async Task<IActionResult> GetOrganizers()
    {
        List<OrganizerResponse> result = await _userService.GetOrganizersAsync();
        return SuccessResponse(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userService.DeleteUserAsync(id);
        return SuccessResponse("User deleted successfully.");
    }

    [HttpPut("{id}/remove-roles")]
    public async Task<IActionResult> RemoveUserRoles(int id, [FromBody] RemoveUserRolesRequest request)
    {
        await _userService.RemoveUserRolesAsync(id, request.RoleIds);
        return SuccessResponse("Roles removed successfully.");
    }
}
