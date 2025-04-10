using DapperSamples.Models;

namespace DapperSamples.Data.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetByIdWithCategoryAsync(int id);
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<int> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Product>> GetPagedProductsAsync(int page, int pageSize);
    Task<int> GetTotalProductCountAsync();
} 