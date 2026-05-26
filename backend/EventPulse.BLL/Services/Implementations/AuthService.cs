using System.Security.Cryptography;
using System.Text;
using EventPulse.BLL.DTOs.Auth;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

            Role role = await authRepository.GetRoleByNameAsync(request.Role)
                ?? throw new BadRequestException($"Role '{request.Role}' not found.");

            User? existingUser = await authRepository.GetUserWithRolesByEmailAsync(normalizedEmail);

            User user;
            List<string> allRoles;

            if (existingUser != null)
            {
                user = existingUser;
                allRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

                if (allRoles.Contains(role.Name))
                    throw new BadRequestException($"You already have the '{role.Name}' role.");

                await authRepository.AddUserRoleAsync(new UserRole { UserId = user.Id, RoleId = role.Id });
                allRoles.Add(role.Name);
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
                allRoles = new List<string> { role.Name };
            }

            try
            {
                await unitOfWork.SaveAsync();
            }
            catch (DbUpdateException)
            {
                throw new BadRequestException("Email is already in use.");
            }

            string accessToken = jwtService.GenerateAccessToken(user, allRoles);
            string refreshToken = jwtService.GenerateRefreshToken();
            int refreshMinutes = jwtService.GetRefreshTokenExpirationMinutes(allRoles);

            await authRepository.AddRefreshTokenAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(refreshMinutes),
                CreatedAt = DateTime.UtcNow,
            });

            await unitOfWork.SaveAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = jwtService.GetAccessTokenExpirationMinutes(allRoles) * 60,
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

            if (!user.UserRoles.Any(ur =>
                ur.Role.Name.Equals(request.Role, StringComparison.OrdinalIgnoreCase)))
                throw new UnauthorizedAccessException("Invalid email or password.");

            List<string> roles = new List<string> { request.Role };
            string accessToken = jwtService.GenerateAccessToken(user, roles);
            string refreshToken = jwtService.GenerateRefreshToken();
            int refreshMinutes = jwtService.GetRefreshTokenExpirationMinutes(roles);

            await authRepository.AddRefreshTokenAsync(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(refreshMinutes),
                CreatedAt = DateTime.UtcNow,
                Role = request.Role,
            });

            await unitOfWork.SaveAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = jwtService.GetAccessTokenExpirationMinutes(roles) * 60,
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
            List<string> roles = string.IsNullOrEmpty(storedToken.Role)
                ? user.UserRoles.Select(ur => ur.Role.Name).ToList()
                : new List<string> { storedToken.Role };
            string newAccessToken = jwtService.GenerateAccessToken(user, roles);
            string newRefreshToken = jwtService.GenerateRefreshToken();
            int refreshMinutes = jwtService.GetRefreshTokenExpirationMinutes(roles);

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
                ExpiresIn = jwtService.GetAccessTokenExpirationMinutes(roles) * 60,
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
