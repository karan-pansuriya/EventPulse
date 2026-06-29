using EventPulse.BLL.DTOs.Auth;
using EventPulse.BLL.Interfaces;
using EventPulse.BLL.Models.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            TokenResponse result = await _authService.RegisterAsync(request);
            SetTokenCookies(result);
            ApiResponse<TokenResponse> response = new ApiResponse<TokenResponse>(true, 200, "Registration successful.", result);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            TokenResponse result = await _authService.LoginAsync(request);
            SetTokenCookies(result);
            ApiResponse<TokenResponse> response = new ApiResponse<TokenResponse>(true, 200, "Login successful.", result);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            TokenResponse result = await _authService.RefreshTokenAsync(request);
            SetTokenCookies(result);
            ApiResponse<TokenResponse> response = new ApiResponse<TokenResponse>(true, 200, "Token refreshed.", result);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            List<RoleResponse> roles = await _authService.GetRolesAsync();
            ApiResponse<List<RoleResponse>> response = new ApiResponse<List<RoleResponse>>(true, 200, "Roles retrieved successfully.", roles);
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
            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            };

            Response.Cookies.Delete("access_token", cookieOptions);
            Response.Cookies.Delete("refresh_token", cookieOptions);
        }
    }
}
