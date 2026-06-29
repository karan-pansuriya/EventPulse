using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
