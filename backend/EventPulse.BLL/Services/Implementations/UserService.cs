using EventPulse.BLL.DTOs.User;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<UserListResponse>> GetPagedUsersAsync(PageRequest pageRequest, int? roleId = null)
    {
        return await _userRepository.GetPagedUsersAsync(pageRequest, roleId);
    }

    public async Task<List<OrganizerResponse>> GetOrganizersAsync()
    {
        return await _userRepository.GetOrganizersAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        await _userRepository.DeleteUserAsync(id);
        await _unitOfWork.SaveAsync();
    }

    public async Task RemoveUserRolesAsync(int userId, List<int> roleIds)
    {
        await _userRepository.RemoveUserRolesAsync(userId, roleIds);
        await _unitOfWork.SaveAsync();
    }
}
