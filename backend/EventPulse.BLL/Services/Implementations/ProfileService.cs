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

        string name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Name is required.");
        if (name.Length < 2)
            throw new BadRequestException("Name must be at least 2 characters.");
        if (name.Length > 100)
            throw new BadRequestException("Name must be at most 100 characters.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z0-9\s.,'\-]+$"))
            throw new BadRequestException("Name contains invalid characters.");

        string? phone = request.Phone?.Trim();
        if (!string.IsNullOrEmpty(phone) && !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[0-9]{6,15}$"))
            throw new BadRequestException("Invalid phone number format.");

        user.Name = name;
        user.Phone = phone;

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
