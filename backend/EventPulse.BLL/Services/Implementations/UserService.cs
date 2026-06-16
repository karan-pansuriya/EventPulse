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
        PagedResult<UserListResponse> paged = await _userRepository.GetPagedUsersAsync(pageRequest, roleId);

        return paged;
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
