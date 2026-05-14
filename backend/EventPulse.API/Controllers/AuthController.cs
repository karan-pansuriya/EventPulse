using EventPulse.BLL.DTOs.Auth;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await authService.RegisterAsync(request);
            return Ok(new { success = true, statusCode = 200, message = "Registration successful.", data = result });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await authService.LoginAsync(request);
            return Ok(new { success = true, statusCode = 200, message = "Login successful.", data = result });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await authService.RefreshTokenAsync(request);
            return Ok(new { success = true, statusCode = 200, message = "Token refreshed.", data = result });
        }
    }
}
