using DapperSamples.DTOs;
using DapperSamples.Services;
using Microsoft.AspNetCore.Mvc;

namespace DapperSamples.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailDto>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        
        if (product == null)
            return NotFound();
        
        return Ok(product);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResult<ProductDto>>> GetPagedProducts(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        var products = await _productService.GetPagedProductsAsync(page, pageSize);
        return Ok(products);
    }

    [HttpGet("category/{categoryId}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
    {
        var products = await _productService.GetProductsByCategoryAsync(categoryId);
        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateProduct(CreateProductDto productDto)
    {
        var id = await _productService.CreateProductAsync(productDto);
        return CreatedAtAction(nameof(GetProduct), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto productDto)
    {
        if (id != productDto.Id)
            return BadRequest();
        
        var result = await _productService.UpdateProductAsync(productDto);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var result = await _productService.DeleteProductAsync(id);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }
} 