using EventPulse.BLL.DTOs.Auth;
using EventPulse.BLL.Interfaces;
using EventPulse.BLL.Models.Request;
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
            var response = new ApiResponse<TokenResponse>(true, 200, "Registration successful.", result);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await authService.LoginAsync(request);
            var response = new ApiResponse<TokenResponse>(true, 200, "Login successful.", result);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var result = await authService.RefreshTokenAsync(request);
            var response = new ApiResponse<TokenResponse>(true, 200, "Token refreshed.", result);
            return Ok(response);
        }
    }
}
