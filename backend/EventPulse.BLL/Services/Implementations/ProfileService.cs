using EventPulse.BLL.DTOs.User;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventPulse.BLL.Services;

public class ProfileService : BaseService, IProfileService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileService(
        IGenericRepository<User> userRepo,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
        _userRepo = userRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDto> GetProfileAsync()
    {
        int userId = GetUserId();
        User? user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found.");

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            CreatedAt = user.CreatedAt,
        };
    }

    public async Task<UserDto> UpdateProfileAsync(UpdateUserRequest request)
    {
        int userId = GetUserId();
        User? user = await _userRepo.GetByIdAsync(userId)
            ?? throw new NotFoundException("User not found.");

        user.Name = request.Name;
        user.Phone = request.Phone;

        _userRepo.Update(user);
        await _unitOfWork.SaveAsync();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            CreatedAt = user.CreatedAt,
        };
    }
}
