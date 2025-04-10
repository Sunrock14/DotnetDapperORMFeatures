namespace DapperSamples.DTOs;

public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public static PaginatedResult<T> Create(IEnumerable<T> items, int page, int pageSize, int totalCount)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PaginatedResult<T>(items, page, pageSize, totalCount, totalPages);
    }
}; 