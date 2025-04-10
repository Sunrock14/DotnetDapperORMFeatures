namespace DapperSamples.DTOs;

public record ProductDto(
    int Id,
    string Name,
    decimal Price,
    string Description,
    int CategoryId,
    string? CategoryName,
    bool IsActive,
    DateTime CreatedDate);

public record CreateProductDto(
    string Name,
    decimal Price, 
    string Description,
    int CategoryId);

public record UpdateProductDto(
    int Id,
    string Name,
    decimal Price, 
    string Description,
    int CategoryId,
    bool IsActive);

public record ProductDetailDto(
    int Id,
    string Name,
    decimal Price,
    string Description,
    DateTime CreatedDate,
    CategoryDto Category);

public record ProductListDto(
    int Id,
    string Name,
    decimal Price,
    string CategoryName); 