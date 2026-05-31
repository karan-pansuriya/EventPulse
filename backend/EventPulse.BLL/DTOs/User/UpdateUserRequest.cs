using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.User;

public class UpdateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Phone]
    public string? Phone { get; set; }
}
