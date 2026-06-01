using EventPulse.BLL.DTOs.Category;
using EventPulse.BLL.Exceptions;
using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;
using EventPulse.DAL.Repositories.Interfaces;

namespace EventPulse.BLL.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        IEnumerable<Category> categories = await _categoryRepository.GetAllAsync();

        return categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            ImagePath = c.ImagePath,
        });
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryDto dto)
    {
        Category category = new Category
        {
            Name = dto.Name,
            ImagePath = dto.ImagePath,
        };

        await _categoryRepository.AddAsync(category);
        await _unitOfWork.SaveAsync();

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ImagePath = category.ImagePath,
        };
    }

    public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");

        category.Name = dto.Name;
        category.ImagePath = dto.ImagePath;

        _categoryRepository.Update(category);
        await _unitOfWork.SaveAsync();

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ImagePath = category.ImagePath,
        };
    }

    public async Task DeleteAsync(int id)
    {
        Category? category = await _categoryRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");

        _categoryRepository.Delete(category);
        await _unitOfWork.SaveAsync();
    }
}
