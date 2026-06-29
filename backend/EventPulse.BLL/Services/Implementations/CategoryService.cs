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

    public async Task<IEnumerable<CategoryResponse>> GetAllCategorysAsync()
    {
        return await _categoryRepository.GetAllCategorysAsync();
    }

    public async Task CreateCategoryAsync(CreateCategoryDto dto)
    {
        if (await _categoryRepository.CategoryNameExistsAsync(dto.Name))
            throw new BadRequestException("A category with this name already exists.");

        Category category = new Category
        {
            Name = dto.Name,
        };

        await _categoryRepository.AddCategoryAsync(category);
        await _unitOfWork.SaveAsync();
    }

    public async Task UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        Category? category = await _categoryRepository.GetCategoryByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");

        if (await _categoryRepository.CategoryNameExistsAsync(dto.Name))
            throw new BadRequestException("A category with this name already exists.");

        category.Name = dto.Name;

        _categoryRepository.UpdateCategory(category);
        await _unitOfWork.SaveAsync();
    }

    public async Task DeleteCategoryAsync(int id)
    {
        Category? category = await _categoryRepository.GetCategoryByIdAsync(id)
            ?? throw new NotFoundException("Category not found.");

        _categoryRepository.DeleteCategory(category);
        await _unitOfWork.SaveAsync();
    }
}
