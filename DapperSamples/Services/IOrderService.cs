using DapperSamples.DTOs;
using DapperSamples.Models;

namespace DapperSamples.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDetailDto?> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status);
    Task<int> CreateOrderAsync(CreateOrderDto order);
    Task<bool> UpdateOrderAsync(UpdateOrderDto order);
    Task<bool> DeleteOrderAsync(int id);
    Task<bool> UpdateOrderStatusAsync(OrderStatusUpdateDto statusUpdate);
    Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
} 