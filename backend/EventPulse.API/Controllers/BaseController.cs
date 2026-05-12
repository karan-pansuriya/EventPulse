using EventPulse.API.Models.Request;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace EventPulse.API.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected int GetCustomerId()
        {
            // Customer tokens use JwtRegisteredClaimNames.Sub ("sub")
            // Admin tokens use ClaimTypes.NameIdentifier (long MS URI)
            var claim = User.FindFirst(JwtRegisteredClaimNames.Sub)
                     ?? User.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null || !int.TryParse(claim.Value, out int id))
                throw new UnauthorizedAccessException("Customer ID not found in token");

            return id;
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
