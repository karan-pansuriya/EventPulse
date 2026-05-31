using AutoMapper;
using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.Category;
using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Route("api/categories")]
public class CategoriesController : BaseHelper
{
    private readonly IGenericService<Category> _categoryService;
    private readonly IMapper _mapper;

    public CategoriesController(IGenericService<Category> categoryService, IMapper mapper)
    {
        _categoryService = categoryService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        var result = _mapper.Map<IEnumerable<CategoryResponse>>(categories);
        return SuccessResponse(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            ImagePath = dto.ImagePath,
        };
        var created = await _categoryService.AddAsync(category);
        var result = _mapper.Map<CategoryResponse>(created);
        return CreatedResponse(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _categoryService.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Category not found.");

        category.Name = dto.Name;
        category.ImagePath = dto.ImagePath;

        var updated = await _categoryService.UpdateAsync(id, category);
        var result = _mapper.Map<CategoryResponse>(updated);
        return SuccessResponse(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        return SuccessResponse("Category deleted successfully.");
    }
}
