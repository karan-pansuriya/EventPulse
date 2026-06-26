using System.Security.Cryptography;
using System.Text;
using EventPulse.BLL.Common;
using EventPulse.BLL.DTOs.Auth;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.BLL.Services
{
    public class AuthService(
        IAuthRepository authRepository,
        JwtService jwtService,
        IUnitOfWork unitOfWork) : IAuthService
    {
        public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();

            Role role = await authRepository.GetRoleByIdAsync(request.RoleId)
                ?? throw new BadRequestException($"Role with ID '{request.RoleId}' not found.");

            User? existingUser = await authRepository.GetUserWithRolesByEmailAsync(normalizedEmail);

            User user;
            List<int> roleIds;

            if (existingUser != null)
            {
                if (existingUser.IsDeleted)
                {
                    existingUser.IsDeleted = false;
                    existingUser.UpdatedAt = DateTime.UtcNow;

                    await authRepository.UpdateUserAsync(existingUser);
                }
                user = existingUser;
                roleIds = user.UserRoles.Select(ur => ur.Role.Id).ToList();

                if (roleIds.Contains(role.Id))
                    throw new BadRequestException($"You already have the '{role.Name}' role.");

                if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                    throw new BadRequestException("Password does not match the existing account for this email.");

                await authRepository.AddUserRoleAsync(new UserRole { UserId = user.Id, RoleId = role.Id });
                roleIds.Add(role.Id);
            }
            else
            {
                CreatePasswordHash(request.Password, out string hash, out string salt);

                user = new User
                {
                    Name = request.Name,
                    Email = normalizedEmail,
                    Phone = request.Phone,
                    PasswordHash = hash,
                    PasswordSalt = salt,
                };

                await authRepository.AddUserAsync(user);
                await authRepository.AddUserRoleAsync(new UserRole { User = user, Role = role });
                roleIds = new List<int> { role.Id };
            }

            await unitOfWork.SaveAsync();


            string accessToken = jwtService.GenerateAccessToken(user, roleIds);
            string refreshToken = jwtService.GenerateRefreshToken();
            int refreshMinutes = jwtService.GetRefreshTokenExpirationMinutes(roleIds);

            await authRepository.DeleteUserRefreshTokensAsync(user.Id);

            await authRepository.AddRefreshTokenAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(refreshMinutes),
                CreatedAt = DateTime.UtcNow,
                Role = role.Name,
            });

            await unitOfWork.SaveAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = jwtService.GetAccessTokenExpirationMinutes(roleIds) * 60,
                RefreshExpiresIn = refreshMinutes * 60,
            };
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            User user = await authRepository.GetUserWithRolesByEmailAsync(normalizedEmail)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new UnauthorizedAccessException("Invalid email or password.");

            Role? matchedRole = user.UserRoles
                .Select(ur => ur.Role)
                .FirstOrDefault(r => r.Id == request.RoleId);

            if (matchedRole is null)
                throw new ForbiddenException("You do not have access with the selected role.");

            List<int> roleIds = new List<int> { request.RoleId };
            string accessToken = jwtService.GenerateAccessToken(user, roleIds);
            string refreshToken = jwtService.GenerateRefreshToken();
            int refreshMinutes = jwtService.GetRefreshTokenExpirationMinutes(roleIds);

            await authRepository.DeleteUserRefreshTokensAsync(user.Id);

            await authRepository.AddRefreshTokenAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(refreshMinutes),
                CreatedAt = DateTime.UtcNow,
                Role = matchedRole.Name,
            });

            await unitOfWork.SaveAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = jwtService.GetAccessTokenExpirationMinutes(roleIds) * 60,
                RefreshExpiresIn = refreshMinutes * 60,
            };
        }

        public async Task<List<RoleResponse>> GetRolesAsync()
        {
            List<Role> roles = await authRepository.GetRolesAsync();
            return roles.Select(r => new RoleResponse { Id = r.Id, Name = r.Name }).ToList();
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            RefreshToken storedToken = await authRepository.GetRefreshTokenWithUserAsync(request.RefreshToken)
                ?? throw new UnauthorizedAccessException("Invalid refresh token.");

            if (storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token is expired or revoked.");

            storedToken.IsRevoked = true;

            User user = storedToken.User;
            List<int> roleIds;

            if (string.IsNullOrEmpty(storedToken.Role))
            {
                roleIds = user.UserRoles.Select(ur => ur.Role.Id).ToList();
            }
            else
            {
                Role? role = await authRepository.GetRoleByIdAsync(
                    storedToken.Role switch
                    {
                        "Admin" => RoleId.Admin,
                        "Organizer" => RoleId.Organizer,
                        "Customer" => RoleId.Customer,
                        _ => 0
                    });

                roleIds = role is not null
                    ? new List<int> { role.Id }
                    : user.UserRoles.Select(ur => ur.Role.Id).ToList();
            }

            string newAccessToken = jwtService.GenerateAccessToken(user, roleIds);
            string newRefreshToken = jwtService.GenerateRefreshToken();
            int refreshMinutes = jwtService.GetRefreshTokenExpirationMinutes(roleIds);

            await authRepository.DeleteUserRefreshTokensAsync(user.Id);

            await authRepository.AddRefreshTokenAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(refreshMinutes),
                CreatedAt = DateTime.UtcNow,
                Role = storedToken.Role,
            });

            await unitOfWork.SaveAsync();

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = jwtService.GetAccessTokenExpirationMinutes(roleIds) * 60,
                RefreshExpiresIn = refreshMinutes * 60,
            };
        }

        private static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using HMACSHA512 hmac = new HMACSHA512();
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private static bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
        {
            using HMACSHA512 hmac = new HMACSHA512(Convert.FromBase64String(passwordSalt));
            string computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return computedHash == passwordHash;
        }
    }
}
