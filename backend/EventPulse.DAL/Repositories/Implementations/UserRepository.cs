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

    public async Task<PagedResult<User>> GetPagedUsersAsync(PageRequest pageRequest, string? roleName = null)
    {
        IQueryable<User> query = _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted);

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName));
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
}
