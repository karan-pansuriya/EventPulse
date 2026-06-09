using EventPulse.DAL.Entities;

namespace EventPulse.DAL.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllCategorysAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<bool> CategoryNameExistsAsync(string name);

    Task AddCategoryAsync(Category category);
    void UpdateCategory(Category category);
    void DeleteCategory(Category category);
}
