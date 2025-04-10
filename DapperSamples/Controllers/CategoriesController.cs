using DapperSamples.DTOs;
using DapperSamples.Services;
using Microsoft.AspNetCore.Mvc;

namespace DapperSamples.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        
        if (category == null)
            return NotFound();
        
        return Ok(category);
    }
    
    [HttpGet("{id}/products")]
    public async Task<ActionResult<CategoryWithProductsDto>> GetCategoryWithProducts(int id)
    {
        var category = await _categoryService.GetCategoryWithProductsAsync(id);
        
        if (category == null)
            return NotFound();
        
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateCategory(CreateCategoryDto categoryDto)
    {
        var id = await _categoryService.CreateCategoryAsync(categoryDto);
        return CreatedAtAction(nameof(GetCategory), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto categoryDto)
    {
        if (id != categoryDto.Id)
            return BadRequest();
        
        var result = await _categoryService.UpdateCategoryAsync(categoryDto);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }
} 