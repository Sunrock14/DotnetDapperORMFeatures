using DapperSamples.Models;

namespace DapperSamples.Data.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetByIdWithProductsAsync(int id);
    Task<int> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int id);
} 