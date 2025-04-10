namespace DapperSamples.DTOs;

public record OrderItemDto(
    int Id,
    int OrderId,
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public record CreateOrderItemDto(
    int ProductId,
    int Quantity);

public record UpdateOrderItemDto(
    int Id,
    int Quantity); 