using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Entities;

namespace EventPulse.DAL.Repositories.Interfaces;

public interface IUserRepository
{
    Task<PagedResult<User>> GetPagedUsersAsync(PageRequest pageRequest, int? roleId = null);
    Task<List<User>> GetOrganizersAsync();
    Task DeleteUserAsync(int id);
    Task RemoveUserRolesAsync(int userId, List<int> roleIds);
}
