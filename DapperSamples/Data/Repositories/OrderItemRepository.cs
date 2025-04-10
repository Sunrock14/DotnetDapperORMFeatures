using Dapper;
using DapperSamples.Models;

namespace DapperSamples.Data.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;

    public OrderItemRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<OrderItem>> GetAllByOrderIdAsync(int orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Bu sorguda JOIN kullanarak ürün bilgilerini de alıyoruz
        return await connection.QueryAsync<OrderItem>(@"
            SELECT oi.*, p.Name as ProductName 
            FROM OrderItems oi
            JOIN Products p ON oi.ProductId = p.Id
            WHERE oi.OrderId = @OrderId", 
            new { OrderId = orderId });
    }

    public async Task<OrderItem?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // QueryFirstOrDefault - tek bir satır döndürür veya null
        return await connection.QueryFirstOrDefaultAsync<OrderItem>(
            "SELECT * FROM OrderItems WHERE Id = @Id", 
            new { Id = id });
    }

    public async Task<int> CreateAsync(OrderItem orderItem)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Dapper parametreli sorgu ve SCOPE_IDENTITY
        var sql = @"
            INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice, TotalPrice)
            VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @TotalPrice);
            SELECT CAST(SCOPE_IDENTITY() as int)";
        
        // Toplam fiyatı hesaplıyoruz
        orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        
        return await connection.QuerySingleAsync<int>(sql, orderItem);
    }

    public async Task<bool> UpdateAsync(OrderItem orderItem)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Dapper Execute - güncelleme işlemi
        // Toplam fiyatı yeniden hesaplıyoruz
        orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
        
        var rowsAffected = await connection.ExecuteAsync(@"
            UPDATE OrderItems 
            SET Quantity = @Quantity, 
                UnitPrice = @UnitPrice, 
                TotalPrice = @TotalPrice
            WHERE Id = @Id", orderItem);
        
        // Siparişin toplam tutarını da güncelliyoruz
        if (rowsAffected > 0)
        {
            await connection.ExecuteAsync(@"
                UPDATE Orders
                SET TotalAmount = (SELECT SUM(TotalPrice) FROM OrderItems WHERE OrderId = @OrderId)
                WHERE Id = @OrderId", 
                new { OrderId = orderItem.OrderId });
        }
        
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        // Dapper transaction kullanımı
        using var transaction = connection.BeginTransaction();
        
        try
        {
            // Önce silinecek satırın sipariş ID'sini alıyoruz
            var orderItem = await connection.QueryFirstOrDefaultAsync<OrderItem>(
                "SELECT * FROM OrderItems WHERE Id = @Id", 
                new { Id = id }, 
                transaction);
            
            if (orderItem == null)
            {
                transaction.Rollback();
                return false;
            }
            
            // Sipariş öğesini siliyoruz
            var rowsAffected = await connection.ExecuteAsync(
                "DELETE FROM OrderItems WHERE Id = @Id", 
                new { Id = id }, 
                transaction);
            
            // Siparişin toplam tutarını güncelliyoruz
            if (rowsAffected > 0)
            {
                await connection.ExecuteAsync(@"
                    UPDATE Orders
                    SET TotalAmount = COALESCE((SELECT SUM(TotalPrice) FROM OrderItems WHERE OrderId = @OrderId), 0)
                    WHERE Id = @OrderId", 
                    new { OrderId = orderItem.OrderId }, 
                    transaction);
            }
            
            transaction.Commit();
            return rowsAffected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> DeleteAllByOrderIdAsync(int orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var rowsAffected = await connection.ExecuteAsync(
            "DELETE FROM OrderItems WHERE OrderId = @OrderId", 
            new { OrderId = orderId });
        
        // Siparişin toplam tutarını sıfırlıyoruz
        await connection.ExecuteAsync(
            "UPDATE Orders SET TotalAmount = 0 WHERE Id = @OrderId", 
            new { OrderId = orderId });
        
        return rowsAffected > 0;
    }
} 