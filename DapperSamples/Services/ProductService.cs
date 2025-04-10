using DapperSamples.Data.Repositories;
using DapperSamples.DTOs;
using DapperSamples.Models;

namespace DapperSamples.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        
        return products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Price,
            p.Description,
            p.CategoryId,
            p.Category?.Name,
            p.IsActive,
            p.CreatedDate
        ));
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id);
        
        if (product == null)
            return null;
        
        return new ProductDetailDto(
            product.Id,
            product.Name,
            product.Price,
            product.Description,
            product.CreatedDate,
            new CategoryDto(
                product.Category!.Id,
                product.Category.Name,
                product.Category.Description,
                product.Category.IsActive
            )
        );
    }
    
    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _productRepository.GetByCategoryAsync(categoryId);
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        
        return products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Price,
            p.Description,
            p.CategoryId,
            category?.Name,
            p.IsActive,
            p.CreatedDate
        ));
    }

    public async Task<PaginatedResult<ProductDto>> GetPagedProductsAsync(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        
        var products = await _productRepository.GetPagedProductsAsync(page, pageSize);
        var totalCount = await _productRepository.GetTotalProductCountAsync();
        
        var productDtos = products.Select(p => new ProductDto(
            p.Id,
            p.Name,
            p.Price,
            p.Description,
            p.CategoryId,
            p.Category?.Name,
            p.IsActive,
            p.CreatedDate
        ));
        
        return PaginatedResult<ProductDto>.Create(productDtos, page, pageSize, totalCount);
    }

    public async Task<int> CreateProductAsync(CreateProductDto productDto)
    {
        var product = new Product
        {
            Name = productDto.Name,
            Price = productDto.Price,
            Description = productDto.Description,
            CategoryId = productDto.CategoryId,
            CreatedDate = DateTime.Now,
            IsActive = true
        };
        
        return await _productRepository.CreateAsync(product);
    }

    public async Task<bool> UpdateProductAsync(UpdateProductDto productDto)
    {
        var existingProduct = await _productRepository.GetByIdAsync(productDto.Id);
        
        if (existingProduct == null)
            return false;
        
        existingProduct.Name = productDto.Name;
        existingProduct.Price = productDto.Price;
        existingProduct.Description = productDto.Description;
        existingProduct.CategoryId = productDto.CategoryId;
        existingProduct.IsActive = productDto.IsActive;
        
        return await _productRepository.UpdateAsync(existingProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        return await _productRepository.DeleteAsync(id);
    }
} 