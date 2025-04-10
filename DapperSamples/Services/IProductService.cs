using DapperSamples.DTOs;

namespace DapperSamples.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDetailDto?> GetProductByIdAsync(int id);
    Task<PaginatedResult<ProductDto>> GetPagedProductsAsync(int page, int pageSize);
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    Task<int> CreateProductAsync(CreateProductDto product);
    Task<bool> UpdateProductAsync(UpdateProductDto product);
    Task<bool> DeleteProductAsync(int id);
} 