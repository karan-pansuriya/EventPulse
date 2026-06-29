using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.User;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters.")]
    [MaxLength(100, ErrorMessage = "Name must be at most 100 characters.")]
    [RegularExpression(@"^[A-Za-z0-9\s.,'\-]{2,100}$", ErrorMessage = "Name contains invalid characters.")]
    public string Name { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format.")]
    [MaxLength(10, ErrorMessage = "Phone number must be at most 10 digits.")]
    public string? Phone { get; set; }
}
