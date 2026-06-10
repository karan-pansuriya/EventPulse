using EventPulse.BLL.DTOs.User;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserListResponse>> GetPagedUsersAsync(PageRequest pageRequest, int? roleId = null)
    {
        PagedResult<User> paged = await _userRepository.GetPagedUsersAsync(pageRequest, roleId);

        var result = new PagedResult<UserListResponse>
        {
            Items = paged.Items.Select(u => new UserListResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Phone = u.Phone,
                IsActive = u.IsActive,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                RoleIds = u.UserRoles.Select(ur => ur.Role.Id).ToList(),
                CreatedAt = u.CreatedAt,
            }),
            TotalCount = paged.TotalCount,
        };

        return result;
    }

    public async Task<List<OrganizerResponse>> GetOrganizersAsync()
    {
        List<User> organizers = await _userRepository.GetOrganizersAsync();

        return organizers.Select(u => new OrganizerResponse
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
        }).ToList();
    }

    public async Task DeleteUserAsync(int id)
    {
        await _userRepository.DeleteUserAsync(id);
    }

    public async Task RemoveUserRolesAsync(int userId, List<int> roleIds)
    {
        await _userRepository.RemoveUserRolesAsync(userId, roleIds);
    }
}
