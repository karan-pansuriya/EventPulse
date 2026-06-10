using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Context;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.DAL.Repositories.Implementations;

public class UserRepository(EventPulseDbContext context) : IUserRepository
{
    private readonly EventPulseDbContext _context = context;

    public async Task<PagedResult<User>> GetPagedUsersAsync(PageRequest pageRequest, int? roleId = null)
    {
        IQueryable<User> query = _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted);

        if (roleId.HasValue)
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Id == roleId.Value));
        }

        int totalCount = await query.CountAsync();

        List<User> items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((pageRequest.PageNumber - 1) * pageRequest.PageSize)
            .Take(pageRequest.PageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async Task<List<User>> GetOrganizersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.UserRoles.Any(ur => ur.Role.Id == 2) && !u.IsDeleted)
            .ToListAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        if (user is null) return;

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserRolesAsync(int userId, List<int> roleIds)
    {
        User? user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user is null) return;

        List<UserRole> toRemove = user.UserRoles.Where(ur => roleIds.Contains(ur.RoleId)).ToList();
        if (toRemove.Count == 0) return;

        _context.UserRoles.RemoveRange(toRemove);

        if (user.UserRoles.Count == toRemove.Count)
        {
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}
