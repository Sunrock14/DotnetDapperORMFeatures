using DapperSamples.Data.Repositories;
using DapperSamples.DTOs;
using DapperSamples.Models;

namespace DapperSamples.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(
        IOrderRepository orderRepository, 
        IOrderItemRepository orderItemRepository,
        IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        
        return orders.Select(o => new OrderDto(
            o.Id,
            o.CustomerName,
            o.ContactEmail,
            o.TotalAmount,
            o.OrderDate,
            o.Status
        ));
    }

    public async Task<OrderDetailDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(id);
        
        if (order == null)
            return null;
        
        var itemDtos = order.Items.Select(i => new OrderItemDto(
            i.Id,
            i.OrderId,
            i.ProductId,
            i.Product?.Name ?? "Unknown Product",
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice
        ));
        
        return new OrderDetailDto(
            order.Id,
            order.CustomerName,
            order.ContactEmail,
            order.TotalAmount,
            order.OrderDate,
            order.Status,
            order.Status.ToString(),
            itemDtos
        );
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(OrderStatus status)
    {
        var orders = await _orderRepository.GetOrdersByStatusAsync(status);
        
        return orders.Select(o => new OrderDto(
            o.Id,
            o.CustomerName,
            o.ContactEmail,
            o.TotalAmount,
            o.OrderDate,
            o.Status
        ));
    }

    public async Task<int> CreateOrderAsync(CreateOrderDto orderDto)
    {
        // Yeni sipariş oluşturuyoruz
        var order = new Order
        {
            CustomerName = orderDto.CustomerName,
            ContactEmail = orderDto.ContactEmail,
            TotalAmount = 0, // Başlangıçta 0, sipariş öğeleri ekleyince güncellenecek
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending
        };
        
        // Sipariş kaydediliyor ve ID'si alınıyor
        var orderId = await _orderRepository.CreateAsync(order);
        
        if (orderId > 0 && orderDto.Items.Count > 0)
        {
            decimal totalAmount = 0;
            
            // Sipariş öğelerini ekliyoruz
            foreach (var itemDto in orderDto.Items)
            {
                // Ürün bilgilerini alıyoruz
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                
                if (product != null)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = orderId,
                        ProductId = itemDto.ProductId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * itemDto.Quantity
                    };
                    
                    await _orderItemRepository.CreateAsync(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }
            }
            
            // Siparişin toplam tutarını güncelliyoruz
            if (totalAmount > 0)
            {
                order.Id = orderId;
                order.TotalAmount = totalAmount;
                await _orderRepository.UpdateAsync(order);
            }
        }
        
        return orderId;
    }

    public async Task<bool> UpdateOrderAsync(UpdateOrderDto orderDto)
    {
        var existingOrder = await _orderRepository.GetByIdAsync(orderDto.Id);
        
        if (existingOrder == null)
            return false;
        
        existingOrder.CustomerName = orderDto.CustomerName;
        existingOrder.ContactEmail = orderDto.ContactEmail;
        existingOrder.Status = orderDto.Status;
        
        return await _orderRepository.UpdateAsync(existingOrder);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        return await _orderRepository.DeleteAsync(id);
    }

    public async Task<bool> UpdateOrderStatusAsync(OrderStatusUpdateDto statusUpdate)
    {
        return await _orderRepository.UpdateOrderStatusAsync(statusUpdate.Id, statusUpdate.Status);
    }

    public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        return await _orderRepository.GetTotalSalesAsync(startDate, endDate);
    }
} 