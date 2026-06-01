using EventPulse.DAL.Context;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.DAL.Repositories.Implementations;

public class CategoryRepository(EventPulseDbContext context) : ICategoryRepository
{
    private readonly EventPulseDbContext _context = context;

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task AddAsync(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        await _context.Categories.AddAsync(category);
    }

    public void Update(Category category)
    {
        category.UpdatedAt = DateTime.UtcNow;
        _context.Categories.Update(category);
    }

    public void Delete(Category category)
    {
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        _context.Categories.Update(category);
    }
}
