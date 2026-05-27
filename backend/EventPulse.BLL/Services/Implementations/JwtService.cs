using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EventPulse.BLL.Common;
using EventPulse.BLL.Models.Configuration;
using EventPulse.DAL.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EventPulse.BLL.Services
{
    public class JwtService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
        }

        public string GenerateAccessToken(User user, IEnumerable<int> roleIds)
        {
            int accessMinutes = GetAccessTokenExpirationMinutes(roleIds);
            List<Claim> claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name, user.Name),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (int roleId in roleIds)
            {
                claims.Add(new Claim("role_id", roleId.ToString()));
                claims.Add(new Claim(ClaimTypes.Role, roleId.ToString()));
            }

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            byte[] randomBytes = new byte[64];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

    public int GetAccessTokenExpirationMinutes(IEnumerable<int> roleIds)
    {
        return IsCustomer(roleIds) ? 24 * 60 : 30;
    }

    public int GetRefreshTokenExpirationMinutes(IEnumerable<int> roleIds)
    {
        return IsCustomer(roleIds) ? 24 * 60 : 30;
    }

        private static bool IsCustomer(IEnumerable<int> roleIds)
        {
            return roleIds.Any(id => id == RoleId.Customer);
        }
    }
}
