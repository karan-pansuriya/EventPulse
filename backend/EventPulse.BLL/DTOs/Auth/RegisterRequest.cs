using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [Required]
        [RegularExpression("^(Admin|Organizer|Customer)$")]
        public string Role { get; set; } = "Customer";
    }
}
