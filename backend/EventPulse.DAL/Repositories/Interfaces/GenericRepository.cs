using System.Linq.Expressions;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Models.Request;
using EventPulse.DAL.Models.Response;
using Microsoft.EntityFrameworkCore;

namespace EventPulse.DAL.Repositories.Interfaces;

public class GenericRepository<T>(DbContext context) : IGenericRepository<T> where T : BaseEntity
{
    private readonly DbContext _context = context;
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.Where(e => !e.IsDeleted).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.SingleOrDefaultAsync(predicate);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task AddAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    public async Task<PagedResult<TResult>> GetPagedAsync<TResult>(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, TResult>> selector,
        PageRequest pageRequest)
    {
        IQueryable<T> query = _dbSet.Where(e => !e.IsDeleted);

        if (predicate != null)
            query = query.Where(predicate);

        if (!string.IsNullOrWhiteSpace(pageRequest.SortBy))
        {
            var property = typeof(T).GetProperty(pageRequest.SortBy);
            if (property != null)
            {
                query = pageRequest.SortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(e => EF.Property<object>(e, pageRequest.SortBy))
                    : query.OrderBy(e => EF.Property<object>(e, pageRequest.SortBy));
            }
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageRequest.PageNumber - 1) * pageRequest.PageSize)
            .Take(pageRequest.PageSize)
            .Select(selector)
            .ToListAsync();

        return new PagedResult<TResult>
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}
