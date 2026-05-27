using EventPulse.DAL.Entities;

namespace EventPulse.DAL.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<bool> UserEmailExistsAsync(string normalizedEmail);

    Task<Role?> GetRoleByIdAsync(int roleId);

    Task<List<Role>> GetRolesAsync();

    Task<User?> GetUserWithRolesByEmailAsync(string normalizedEmail);

    Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token);

    Task AddUserAsync(User user);

    Task AddUserRoleAsync(UserRole userRole);

    Task AddRefreshTokenAsync(RefreshToken token);
}
