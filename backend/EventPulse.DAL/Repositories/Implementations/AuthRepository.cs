using EventPulse.DAL.Context;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.DAL.Repositories.Implementations;

public class AuthRepository(EventPulseDbContext context) : IAuthRepository
{
    private readonly EventPulseDbContext _context = context;

    public async Task<bool> UserEmailExistsAsync(string normalizedEmail)
    {
        return await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task<Role?> GetRoleByIdAsync(int roleId)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<User?> GetUserWithRolesByEmailAsync(string normalizedEmail)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
    }

    public async Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task AddUserRoleAsync(UserRole userRole)
    {
        await _context.UserRoles.AddAsync(userRole);
    }

    public async Task AddRefreshTokenAsync(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
    }
    
}
