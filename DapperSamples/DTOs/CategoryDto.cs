namespace DapperSamples.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string Description,
    bool IsActive);

public record CreateCategoryDto(
    string Name,
    string Description);

public record UpdateCategoryDto(
    int Id,
    string Name,
    string Description,
    bool IsActive);

public record CategoryWithProductsDto(
    int Id,
    string Name,
    string Description,
    bool IsActive,
    IEnumerable<ProductListDto> Products); 