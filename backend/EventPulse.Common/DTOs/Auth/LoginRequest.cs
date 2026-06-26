using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.Auth
{
public class LoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(255)]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Password must be at least 8 characters and include upper, lower, number, and special character.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Range(1, 3)]
    public int RoleId { get; set; }
}
}
