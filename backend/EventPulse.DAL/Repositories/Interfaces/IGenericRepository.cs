using System.Linq.Expressions;
using EventPulse.Common.Entities;
using EventPulse.DAL.Entities;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;

namespace EventPulse.DAL.Repositories.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();

    Task<T?> GetByIdAsync(int id);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    Task AddAsync(T entity);

    void Update(T entity);

    void Delete(T entity);
    
    Task<PagedResult<TResult>> GetPagedAsync<TResult>(
        Expression<Func<T, bool>>? predicate,
        Expression<Func<T, TResult>> selector,
        PageRequest pageRequest);
}
