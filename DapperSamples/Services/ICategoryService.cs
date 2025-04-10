using DapperSamples.DTOs;

namespace DapperSamples.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryWithProductsDto?> GetCategoryWithProductsAsync(int id);
    Task<int> CreateCategoryAsync(CreateCategoryDto category);
    Task<bool> UpdateCategoryAsync(UpdateCategoryDto category);
    Task<bool> DeleteCategoryAsync(int id);
} 