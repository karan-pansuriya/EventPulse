using EventPulse.BLL.Models.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventPulse.API.Helpers
{
    [ApiController]
    public class BaseHelper : ControllerBase
    {
        protected int GetUserId()
        {
            Claim? claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub");
            if (claim == null || !int.TryParse(claim.Value, out int id))
                throw new UnauthorizedAccessException("User ID not found in token.");
            return id;
        }

        protected string? GetUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value;
        }

        protected List<int> GetUserRoleIds()
        {
            return User.FindAll("role_id").Select(c =>
            {
                int.TryParse(c.Value, out int id);
                return id;
            }).Where(id => id > 0).ToList();
        }

        protected bool IsInRole(int roleId)
        {
            return GetUserRoleIds().Contains(roleId);
        }

        // Returns the role the user explicitly logged in with.
        protected int? GetActiveRoleId()
        {
            string? value = User.FindFirst("active_role_id")?.Value;
            if (int.TryParse(value, out int roleId) && roleId > 0)
                return roleId;
            return null;
        }

        protected IActionResult SuccessResponse<T>(T? data, string message = ApiMessages.RequestSuccessful)
        {
            ApiResponse<T> response = new ApiResponse<T>(
                true,
                StatusCodes.Status200OK,
                message,
                data
            );
            return Ok(response);
        }

        protected IActionResult SuccessResponse(string message)
        {
            ApiResponse<object> response = new ApiResponse<object>(
                true,
                StatusCodes.Status200OK,
                message,
                null
            );
            return Ok(response);
        }

        protected IActionResult CreatedResponse<T>(T? data, string message = ApiMessages.CreatedSuccessfully)
        {
            ApiResponse<T> response = new ApiResponse<T>(
                true,
                StatusCodes.Status201Created,
                message,
                data
            );
            return StatusCode(StatusCodes.Status201Created, response);
        }

        protected IActionResult UnauthorizedResponse(string message)
        {
            ApiResponse<object> response = new ApiResponse<object>(
                false,
                StatusCodes.Status401Unauthorized,
                message
            );
            return Unauthorized(response);
        }

        protected IActionResult NotFoundResponse(string message)
        {
            return NotFound(new ApiResponse<object>(
                false,
                StatusCodes.Status404NotFound,
                message
            ));
        }
    }
}