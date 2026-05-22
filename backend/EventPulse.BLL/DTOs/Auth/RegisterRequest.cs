using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Password must be at least 8 characters and include upper, lower, number, and special character.")]
        public string Password { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [Required]
        [RegularExpression("^(Admin|Organizer|Customer)$")]
        public string Role { get; set; } = "Customer";
    }
}
