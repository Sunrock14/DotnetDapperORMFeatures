using Dapper;
using DapperSamples.Models;

namespace DapperSamples.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;

    public CategoryRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        return await connection.QueryAsync<Category>("SELECT * FROM Categories WHERE IsActive = 1");
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        return await connection.QueryFirstOrDefaultAsync<Category>(
            "SELECT * FROM Categories WHERE Id = @Id", 
            new { Id = id });
    }

    public async Task<Category?> GetByIdWithProductsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Çoklu nesne haritalaması (multi-mapping) örneği
        // Kategorileri ve ürünleri birlikte çekip, tek bir sorgu ile ilişkisel veriyi alıyoruz
        var categoryDictionary = new Dictionary<int, Category>();
        
        var categories = await connection.QueryAsync<Category, Product, Category>(
            @"SELECT c.*, p.* 
              FROM Categories c
              LEFT JOIN Products p ON c.Id = p.CategoryId
              WHERE c.Id = @Id AND c.IsActive = 1",
            (category, product) =>
            {
                if (!categoryDictionary.TryGetValue(category.Id, out var currentCategory))
                {
                    currentCategory = category;
                    currentCategory.Products = new List<Product>();
                    categoryDictionary.Add(currentCategory.Id, currentCategory);
                }

                if (product != null && product.Id != 0)
                {
                    currentCategory.Products.Add(product);
                }

                return currentCategory;
            },
            new { Id = id },
            splitOn: "Id"  // Burada hangi kolondan bölüneceğini belirtiyoruz
        );
        
        return categories.FirstOrDefault();
    }

    public async Task<int> CreateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            INSERT INTO Categories (Name, Description, IsActive)
            VALUES (@Name, @Description, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        return await connection.QuerySingleAsync<int>(sql, category);
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var rowsAffected = await connection.ExecuteAsync(@"
            UPDATE Categories 
            SET Name = @Name, 
                Description = @Description, 
                IsActive = @IsActive
            WHERE Id = @Id", category);
        
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Transaction örneği - İlişkili ürünleri ve kategoriyi birlikte soft-delete ediyoruz
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Önce kategoriye ait ürünleri pasif yapıyoruz
            await connection.ExecuteAsync(
                "UPDATE Products SET IsActive = 0 WHERE CategoryId = @Id", 
                new { Id = id }, 
                transaction);
            
            // Sonra kategoriyi pasif yapıyoruz
            var rowsAffected = await connection.ExecuteAsync(
                "UPDATE Categories SET IsActive = 0 WHERE Id = @Id", 
                new { Id = id }, 
                transaction);
            
            transaction.Commit();
            return rowsAffected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
} 