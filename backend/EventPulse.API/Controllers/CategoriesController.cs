using AutoMapper;
using EventPulse.Bll.Helpers;
using EventPulse.BLL.DTOs.Category;
using EventPulse.BLL.Interfaces;
using EventPulse.DAL.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EventPulse.API.Controllers;

[Route("api/categories")]
public class CategoriesController : BaseController
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
}
