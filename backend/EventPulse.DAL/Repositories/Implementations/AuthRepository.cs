using EventPulse.DAL.Context;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.DAL.Repositories.Implementations;

public class AuthRepository(EventPulseDbContext context) : IAuthRepository
{
    private readonly EventPulseDbContext _context = context;

    public async Task<Role?> GetRoleByIdAsync(int roleId)
    {
        return await _context.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        return await _context.Roles.AsNoTracking().ToListAsync();
    }

    public async Task<User?> GetUserWithRolesByEmailAsync(string normalizedEmail)
    {
        return await _context.Users
            .IgnoreQueryFilters()
            .Where(u => u.Email == normalizedEmail)
            .Select(u => new User
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                PasswordHash = u.PasswordHash,
                PasswordSalt = u.PasswordSalt,
                IsDeleted = u.IsDeleted,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                UserRoles = u.UserRoles.Select(ur => new UserRole
                {
                    UserId = ur.UserId,
                    RoleId = ur.RoleId,
                    Role = new Role
                    {
                        Id = ur.Role.Id,
                        Name = ur.Role.Name
                    }
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<RefreshToken?> GetRefreshTokenWithUserAsync(string token)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
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

    public async Task DeleteUserRefreshTokensAsync(int userId)
    {
        List<RefreshToken> tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        if (tokens.Count > 0)
        {
            _context.RefreshTokens.RemoveRange(tokens);
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }
}
