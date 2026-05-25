using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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

        public string GenerateAccessToken(User user, IEnumerable<string> roles)
        {
            int accessMinutes = GetAccessTokenExpirationMinutes(roles);
            List<Claim> claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name, user.Name),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (string role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

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

    public int GetAccessTokenExpirationMinutes(IEnumerable<string> roles)
    {
        return IsCustomer(roles) ? 24 * 60 : 30;
    }

    public int GetRefreshTokenExpirationMinutes(IEnumerable<string> roles)
    {
        return IsCustomer(roles) ? 24 * 60 : 30;
    }

        private static bool IsCustomer(IEnumerable<string> roles)
        {
            return roles.Any(role => string.Equals(role, "Customer", StringComparison.OrdinalIgnoreCase));
        }
    }
}
