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
    public async Task<IActionResult> GetAllCategorys()
    {
        IEnumerable<CategoryResponse> result = await _categoryService.GetAllCategorysAsync();
        return SuccessResponse(result);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        await _categoryService.CreateCategoryAsync(dto);
        return SuccessResponse("Category created successfully.");
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        await _categoryService.UpdateCategoryAsync(id, dto);
        return SuccessResponse("Category Updated successfully.");
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _categoryService.DeleteCategoryAsync(id);
        return SuccessResponse("Category deleted successfully.");
    }
}
