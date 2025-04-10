using DapperSamples.Models;

namespace DapperSamples.Data.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<Order?> GetByIdWithItemsAsync(int id);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task<int> CreateAsync(Order order);
    Task<bool> UpdateAsync(Order order);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateOrderStatusAsync(int id, OrderStatus status);
    Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
} 