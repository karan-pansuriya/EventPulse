using EventPulse.BLL.DTOs.User;

namespace EventPulse.BLL.Interfaces;

public interface IProfileService
{
    Task<UserDto> GetProfileAsync();
    Task<UserDto> UpdateProfileAsync(UpdateUserRequest request);
}
