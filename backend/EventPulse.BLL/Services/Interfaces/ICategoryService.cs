using EventPulse.BLL.DTOs.Category;

namespace EventPulse.BLL.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllCategorysAsync();
    Task<CategoryResponse> CreateCategoryAsync(CreateCategoryDto dto);
    Task<CategoryResponse> UpdateCategoryAsync(int id, UpdateCategoryDto dto);
    Task DeleteCategoryAsync(int id);
}
