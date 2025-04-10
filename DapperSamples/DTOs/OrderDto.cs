using DapperSamples.Models;

namespace DapperSamples.DTOs;

public record OrderDto(
    int Id,
    string CustomerName,
    string ContactEmail,
    decimal TotalAmount,
    DateTime OrderDate,
    OrderStatus Status);

public record CreateOrderDto(
    string CustomerName,
    string ContactEmail,
    List<CreateOrderItemDto> Items);

public record UpdateOrderDto(
    int Id, 
    string CustomerName,
    string ContactEmail,
    OrderStatus Status);

public record OrderDetailDto(
    int Id,
    string CustomerName,
    string ContactEmail,
    decimal TotalAmount,
    DateTime OrderDate,
    OrderStatus Status,
    string StatusText,
    IEnumerable<OrderItemDto> Items);

public record OrderStatusUpdateDto(
    int Id,
    OrderStatus Status); 