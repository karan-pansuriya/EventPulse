using EventPulse.API.Helpers;
using EventPulse.BLL.DTOs.Category;
using EventPulse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Route("api/categories")]
public class CategoriesController : BaseHelper
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<CategoryResponse> result = await _categoryService.GetAllAsync();
        return SuccessResponse(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        CategoryResponse result = await _categoryService.CreateAsync(dto);
        return CreatedResponse(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        CategoryResponse result = await _categoryService.UpdateAsync(id, dto);
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
