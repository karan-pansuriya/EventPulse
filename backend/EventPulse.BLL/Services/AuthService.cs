using System.Security.Cryptography;
using System.Text;
using EventPulse.BLL.DTOs.Auth;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Context;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.BLL.Services
{
    public class AuthService(
        EventPulseDbContext context,
        JwtService jwtService,
        IUnitOfWork unitOfWork) : IAuthService
    {
        public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
        {
            if (await context.Users.AnyAsync(u => u.Email == request.Email))
                throw new BadRequestException("Email is already registered.");

            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == request.Role)
                ?? throw new BadRequestException($"Role '{request.Role}' not found.");

            CreatePasswordHash(request.Password, out var hash, out var salt);

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = hash,
                PasswordSalt = salt,
            };

            context.Users.Add(user);
            context.UserRoles.Add(new UserRole { User = user, Role = role });

            await unitOfWork.SaveAsync();

            var roles = new[] { role.Name };
            var accessToken = jwtService.GenerateAccessToken(user, roles);
            var refreshToken = jwtService.GenerateRefreshToken();

            context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtService.GetRefreshTokenExpirationDays()),
                CreatedAt = DateTime.UtcNow,
            });

            await unitOfWork.SaveAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = jwtService.GetAccessTokenExpirationMinutes() * 60,
            };
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            var user = await context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == request.Email)
                ?? throw new UnauthorizedAccessException("Invalid email or password.");

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var accessToken = jwtService.GenerateAccessToken(user, roles);
            var refreshToken = jwtService.GenerateRefreshToken();

            context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtService.GetRefreshTokenExpirationDays()),
                CreatedAt = DateTime.UtcNow,
            });

            await unitOfWork.SaveAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = jwtService.GetAccessTokenExpirationMinutes() * 60,
            };
        }

        public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var storedToken = await context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken)
                ?? throw new UnauthorizedAccessException("Invalid refresh token.");

            if (storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token is expired or revoked.");

            storedToken.IsRevoked = true;

            var user = storedToken.User;
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            var newAccessToken = jwtService.GenerateAccessToken(user, roles);
            var newRefreshToken = jwtService.GenerateRefreshToken();

            context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(jwtService.GetRefreshTokenExpirationDays()),
                CreatedAt = DateTime.UtcNow,
            });

            await unitOfWork.SaveAsync();

            return new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = jwtService.GetAccessTokenExpirationMinutes() * 60,
            };
        }

        private static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private static bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
        {
            using var hmac = new HMACSHA512(Convert.FromBase64String(passwordSalt));
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return computedHash == passwordHash;
        }
    }
}
