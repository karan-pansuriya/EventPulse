using System.Linq.Expressions;
using System.Security.Claims;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.Common.Entities;
using EventPulse.Common.Models;
using EventPulse.Common.Models.Response;
using EventPulse.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventPulse.BLL.Services
{
    public abstract class BaseService(IHttpContextAccessor httpContextAccessor)
    {
        protected IHttpContextAccessor HttpContextAccessor { get; } = httpContextAccessor;

        protected int GetUserId()
        {
            Claim? claim = HttpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? HttpContextAccessor.HttpContext?.User.FindFirst("sub");
            if (claim == null || !int.TryParse(claim.Value, out int id))
                throw new UnauthorizedAccessException("User ID not found in token.");
            return id;
        }
    }
    public class GenericService<T>(
        IGenericRepository<T> repository,
        IUnitOfWork unitOfWork) : IGenericService<T> where T : BaseEntity
    {
        private readonly IGenericRepository<T> _repository = repository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.FirstOrDefaultAsync(predicate);
        }

        public async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.SingleOrDefaultAsync(predicate);
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _repository.ExistsAsync(predicate);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _repository.AddAsync(entity);
            await _unitOfWork.SaveAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(int id, T entity)
        {
            if (!await _repository.ExistsAsync(x => x.Id == id))
                throw new NotFoundException($"{typeof(T).Name} not found.");

            _repository.Update(entity);
            await _unitOfWork.SaveAsync();

            return entity;
        }

        public async Task DeleteAsync(int id)
        {
            T? entity = await _repository.GetByIdAsync(id) ?? throw new NotFoundException($"{typeof(T).Name} not found.");
            _repository.Delete(entity);
            await _unitOfWork.SaveAsync();
        }

        public async Task<PagedResult<TResult>> GetPagedAsync<TResult>(
            Expression<Func<T, bool>>? predicate,
            Expression<Func<T, TResult>> selector,
            PageRequest pageRequest)
        {
            return await _repository.GetPagedAsync(predicate, selector, pageRequest);
        }
    }
}