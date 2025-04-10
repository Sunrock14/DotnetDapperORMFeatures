using Dapper;
using DapperSamples.Models;
using System.Data;

namespace DapperSamples.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;

    public ProductRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Temel Dapper sorgusu
        return await connection.QueryAsync<Product>(
            "SELECT * FROM Products WHERE IsActive = 1 ORDER BY Name");
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Dapper Parametreli Sorgu
        return await connection.QueryFirstOrDefaultAsync<Product>(
            "SELECT * FROM Products WHERE Id = @Id", 
            new { Id = id });
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Dapper QueryMultiple - Birden fazla sonuç kümesi alma
        var query = @"
            SELECT * FROM Products WHERE Id = @Id;
            SELECT * FROM Categories WHERE Id = (SELECT CategoryId FROM Products WHERE Id = @Id)";
        
        using var multi = await connection.QueryMultipleAsync(query, new { Id = id });
        var product = await multi.ReadFirstOrDefaultAsync<Product>();
        
        if (product != null)
        {
            product.Category = await multi.ReadFirstOrDefaultAsync<Category>();
        }
        
        return product;
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        return await connection.QueryAsync<Product>(
            "SELECT * FROM Products WHERE CategoryId = @CategoryId AND IsActive = 1",
            new { CategoryId = categoryId });
    }

    public async Task<int> CreateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Dapper Execute - INSERT sorgusu çalıştırıp SCOPE_IDENTITY() alarak eklenen kaydın ID'sini döndürme
        var sql = @"
            INSERT INTO Products (Name, Price, Description, CategoryId, CreatedDate, IsActive)
            VALUES (@Name, @Price, @Description, @CategoryId, @CreatedDate, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        return await connection.QuerySingleAsync<int>(sql, product);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Dapper Execute - Etkilenen satır sayısını döndürme
        var rowsAffected = await connection.ExecuteAsync(@"
            UPDATE Products 
            SET Name = @Name, 
                Price = @Price, 
                Description = @Description, 
                CategoryId = @CategoryId,
                IsActive = @IsActive
            WHERE Id = @Id", product);
        
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Soft delete yapıyoruz - sadece IsActive'i false olarak ayarlıyoruz
        var rowsAffected = await connection.ExecuteAsync(
            "UPDATE Products SET IsActive = 0 WHERE Id = @Id", 
            new { Id = id });
        
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<Product>> GetPagedProductsAsync(int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // OFFSET-FETCH ile sayfalama
        var offset = (page - 1) * pageSize;
        
        return await connection.QueryAsync<Product>(@"
            SELECT * FROM Products 
            WHERE IsActive = 1 
            ORDER BY Id
            OFFSET @Offset ROWS 
            FETCH NEXT @PageSize ROWS ONLY", 
            new { Offset = offset, PageSize = pageSize });
    }

    public async Task<int> GetTotalProductCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Tekil değer alma
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Products WHERE IsActive = 1");
    }
} 