using EventPulse.BLL.DTOs.User;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;

namespace EventPulse.BLL.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserListResponse>> GetPagedUsersAsync(PageRequest pageRequest, int? roleId = null);
    Task<List<OrganizerResponse>> GetOrganizersAsync();
    Task DeleteUserAsync(int id);
    Task RemoveUserRolesAsync(int userId, List<int> roleIds);
}
