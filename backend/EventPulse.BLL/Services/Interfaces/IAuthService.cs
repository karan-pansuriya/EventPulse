using EventPulse.BLL.DTOs.Auth;

namespace EventPulse.BLL.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponse> RegisterAsync(RegisterRequest request);
        Task<TokenResponse> LoginAsync(LoginRequest request);
        Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<List<RoleResponse>> GetRolesAsync();
    }
}
