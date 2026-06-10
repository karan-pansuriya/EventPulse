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
            List<int> roleIdList = roleIds.ToList();

            List<Claim> claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name, user.Name),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (int roleId in roleIdList)
            {
                claims.Add(new Claim("role_id", roleId.ToString()));
                claims.Add(new Claim(ClaimTypes.Role, roleId.ToString()));
            }

            // Store the active role — the role the user explicitly logged in with.
            // Since login always passes a single RoleId, first entry is the active role.
            if (roleIdList.Count > 0)
                claims.Add(new Claim("active_role_id", roleIdList[0].ToString()));

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
            // Customer: 60 mins | Admin/Organizer: 15 mins
            return IsCustomer(roleIds) ? 60 : 15;
        }

        public int GetRefreshTokenExpirationMinutes(IEnumerable<int> roleIds)
        {
            // Customer: 30 days | Admin/Organizer: 7 days
            return IsCustomer(roleIds) ? 60 * 24 * 30 : 60 * 24 * 7;
        }

        private static bool IsCustomer(IEnumerable<int> roleIds)
        {
            return roleIds.Any(id => id == RoleId.Customer);
        }
    }
}