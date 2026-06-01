using EventPulse.BLL.DTOs.Category;

namespace EventPulse.BLL.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> CreateAsync(CreateCategoryDto dto);
    Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryDto dto);
    Task DeleteAsync(int id);
}
