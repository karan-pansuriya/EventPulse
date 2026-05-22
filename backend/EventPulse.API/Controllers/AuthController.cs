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
            TokenResponse result = await authService.RegisterAsync(request);
            SetTokenCookies(result);
            ApiResponse<TokenResponse> response = new ApiResponse<TokenResponse>(true, 200, "Registration successful.", result);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            TokenResponse result = await authService.LoginAsync(request);
            SetTokenCookies(result);
            ApiResponse<TokenResponse> response = new ApiResponse<TokenResponse>(true, 200, "Login successful.", result);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            TokenResponse result = await authService.RefreshTokenAsync(request);
            SetTokenCookies(result);
            ApiResponse<TokenResponse> response = new ApiResponse<TokenResponse>(true, 200, "Token refreshed.", result);
            return Ok(response);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            ClearTokenCookies();
            return Ok(new ApiResponse<object>(true, 200, "Logged out successfully.", null));
        }

        private void SetTokenCookies(TokenResponse tokens)
        {
            Response.Cookies.Append("access_token", tokens.AccessToken, new CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.FromSeconds(tokens.ExpiresIn),
            });

            Response.Cookies.Append("refresh_token", tokens.RefreshToken, new CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.FromSeconds(tokens.RefreshExpiresIn),
            });
        }

        private void ClearTokenCookies()
        {
            Response.Cookies.Append("access_token", "", new CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.Zero,
            });

            Response.Cookies.Append("refresh_token", "", new CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.Zero,
            });
        }
    }
}
