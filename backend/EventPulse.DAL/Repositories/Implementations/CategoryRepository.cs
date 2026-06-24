using EventPulse.DAL.Context;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.DAL.Repositories.Implementations;

public class CategoryRepository(EventPulseDbContext context) : ICategoryRepository
{
    private readonly EventPulseDbContext _context = context;

    public async Task<IEnumerable<Category>> GetAllCategorysAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> CategoryNameExistsAsync(string name)
    {
        return await _context.Categories
            .AnyAsync(c => EF.Functions.ILike(c.Name, name));
    }

    public async Task<Category?> GetDeletedCategoryByNameAsync(string name)
    {
        return await _context.Categories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => EF.Functions.ILike(c.Name, name));
    }

    public async Task AddCategoryAsync(Category category)
    {
        category.CreatedAt = DateTime.UtcNow;
        await _context.Categories.AddAsync(category);
    }

    public void UpdateCategory(Category category)
    {
        category.UpdatedAt = DateTime.UtcNow;
        _context.Categories.Update(category);
    }

    public void DeleteCategory(Category category)
    {
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        _context.Categories.Update(category);
    }
}
