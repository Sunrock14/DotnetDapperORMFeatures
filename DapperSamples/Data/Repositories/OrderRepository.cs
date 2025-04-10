using Dapper;
using DapperSamples.Models;
using System.Data;

namespace DapperSamples.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;

    public OrderRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        return await connection.QueryAsync<Order>("SELECT * FROM Orders ORDER BY OrderDate DESC");
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        return await connection.QueryFirstOrDefaultAsync<Order>(
            "SELECT * FROM Orders WHERE Id = @Id", 
            new { Id = id });
    }

    public async Task<Order?> GetByIdWithItemsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // QueryMultiple kullanarak tek seferde birden fazla sonuç kümesi alıyoruz
        var query = @"
            SELECT * FROM Orders WHERE Id = @Id;
            SELECT oi.*, p.Name as ProductName
            FROM OrderItems oi
            JOIN Products p ON oi.ProductId = p.Id
            WHERE oi.OrderId = @Id";
        
        using var multi = await connection.QueryMultipleAsync(query, new { Id = id });
        
        var order = await multi.ReadSingleOrDefaultAsync<Order>();
        if (order != null)
        {
            var orderItems = await multi.ReadAsync<OrderItem>();
            order.Items = orderItems.ToList();
        }
        
        return order;
    }

    public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Enum değerini sorgu parametresi olarak kullanma
        return await connection.QueryAsync<Order>(
            "SELECT * FROM Orders WHERE Status = @Status ORDER BY OrderDate DESC",
            new { Status = (int)status });
    }

    public async Task<int> CreateAsync(Order order)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Dapper ile stored procedure çağırma örneği
        // Parametre olarak DynamicParameters kullanıyoruz
        var parameters = new DynamicParameters();
        parameters.Add("@CustomerName", order.CustomerName);
        parameters.Add("@ContactEmail", order.ContactEmail);
        parameters.Add("@TotalAmount", order.TotalAmount);
        parameters.Add("@OrderDate", order.OrderDate);
        parameters.Add("@Status", (int)order.Status);
        parameters.Add("@OrderId", dbType: DbType.Int32, direction: ParameterDirection.Output);
        
        await connection.ExecuteAsync(
            "CreateOrder",
            parameters,
            commandType: CommandType.StoredProcedure);
        
        // Stored procedure'den dönen çıkış parametresini alıyoruz
        return parameters.Get<int>("@OrderId");
    }

    public async Task<bool> UpdateAsync(Order order)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var rowsAffected = await connection.ExecuteAsync(@"
            UPDATE Orders 
            SET CustomerName = @CustomerName,
                ContactEmail = @ContactEmail,
                TotalAmount = @TotalAmount,
                Status = @Status
            WHERE Id = @Id", 
            new { 
                order.Id, 
                order.CustomerName, 
                order.ContactEmail,
                order.TotalAmount,
                Status = (int)order.Status
            });
        
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Transaction kullanımı - Hem siparişi hem sipariş öğelerini siliyoruz
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Önce sipariş öğelerini siliyoruz
            await connection.ExecuteAsync(
                "DELETE FROM OrderItems WHERE OrderId = @Id", 
                new { Id = id }, 
                transaction);
            
            // Sonra siparişi siliyoruz
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM Orders WHERE Id = @Id", 
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

    public async Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var rowsAffected = await connection.ExecuteAsync(
            "UPDATE Orders SET Status = @Status WHERE Id = @Id",
            new { Id = id, Status = (int)status });
        
        return rowsAffected > 0;
    }

    public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Dinamik SQL oluşturma örneği - Koşullu sorgular
        var sql = "SELECT COALESCE(SUM(TotalAmount), 0) FROM Orders WHERE 1=1";
        var parameters = new DynamicParameters();
        
        if (startDate.HasValue)
        {
            sql += " AND OrderDate >= @StartDate";
            parameters.Add("@StartDate", startDate.Value.Date);
        }
        
        if (endDate.HasValue)
        {
            sql += " AND OrderDate < @EndDate";
            parameters.Add("@EndDate", endDate.Value.Date.AddDays(1)); // Include all orders on the end date
        }
        
        // ExecuteScalar ile tek bir değer dönüyoruz
        return await connection.ExecuteScalarAsync<decimal>(sql, parameters);
    }
} 