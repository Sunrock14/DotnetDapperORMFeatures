using DapperSamples.Data.Repositories;
using DapperSamples.DTOs;
using DapperSamples.Models;

namespace DapperSamples.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        
        return categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.IsActive
        ));
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        
        if (category == null)
            return null;
        
        return new CategoryDto(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive
        );
    }

    public async Task<CategoryWithProductsDto?> GetCategoryWithProductsAsync(int id)
    {
        var category = await _categoryRepository.GetByIdWithProductsAsync(id);
        
        if (category == null)
            return null;
        
        var productDtos = category.Products.Select(p => new ProductListDto(
            p.Id,
            p.Name,
            p.Price,
            category.Name
        ));
        
        return new CategoryWithProductsDto(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive,
            productDtos
        );
    }

    public async Task<int> CreateCategoryAsync(CreateCategoryDto categoryDto)
    {
        var category = new Category
        {
            Name = categoryDto.Name,
            Description = categoryDto.Description,
            IsActive = true
        };
        
        return await _categoryRepository.CreateAsync(category);
    }

    public async Task<bool> UpdateCategoryAsync(UpdateCategoryDto categoryDto)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(categoryDto.Id);
        
        if (existingCategory == null)
            return false;
        
        existingCategory.Name = categoryDto.Name;
        existingCategory.Description = categoryDto.Description;
        existingCategory.IsActive = categoryDto.IsActive;
        
        return await _categoryRepository.UpdateAsync(existingCategory);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await _categoryRepository.DeleteAsync(id);
    }
} 