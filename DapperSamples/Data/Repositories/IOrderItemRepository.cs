using DapperSamples.Models;

namespace DapperSamples.Data.Repositories;

public interface IOrderItemRepository
{
    Task<IEnumerable<OrderItem>> GetAllByOrderIdAsync(int orderId);
    Task<OrderItem?> GetByIdAsync(int id);
    Task<int> CreateAsync(OrderItem orderItem);
    Task<bool> UpdateAsync(OrderItem orderItem);
    Task<bool> DeleteAsync(int id);
    Task<bool> DeleteAllByOrderIdAsync(int orderId);
} 