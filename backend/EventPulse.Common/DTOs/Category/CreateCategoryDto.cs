using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.Category;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [MinLength(1, ErrorMessage = "Category name cannot be empty.")]
    [MaxLength(100, ErrorMessage = "Category name must be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;
}
