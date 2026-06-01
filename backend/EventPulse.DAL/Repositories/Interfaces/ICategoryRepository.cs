using EventPulse.DAL.Entities;

namespace EventPulse.DAL.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task AddAsync(Category category);
    void Update(Category category);
    void Delete(Category category);
}
