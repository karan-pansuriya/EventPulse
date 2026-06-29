using EventPulse.BLL.DTOs.Category;

namespace EventPulse.BLL.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllCategorysAsync();
    Task CreateCategoryAsync(CreateCategoryDto dto);
    Task UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task DeleteCategoryAsync(int id);
}
