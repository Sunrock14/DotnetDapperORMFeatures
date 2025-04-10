using DapperSamples.DTOs;
using DapperSamples.Models;
using DapperSamples.Services;
using Microsoft.AspNetCore.Mvc;

namespace DapperSamples.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetailDto>> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        
        if (order == null)
            return NotFound();
        
        return Ok(order);
    }
    
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByStatus(OrderStatus status)
    {
        var orders = await _orderService.GetOrdersByStatusAsync(status);
        return Ok(orders);
    }
    
    [HttpGet("sales")]
    public async Task<ActionResult<decimal>> GetTotalSales(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        var totalSales = await _orderService.GetTotalSalesAsync(startDate, endDate);
        return Ok(totalSales);
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateOrder(CreateOrderDto orderDto)
    {
        var id = await _orderService.CreateOrderAsync(orderDto);
        return CreatedAtAction(nameof(GetOrder), new { id }, id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, UpdateOrderDto orderDto)
    {
        if (id != orderDto.Id)
            return BadRequest();
        
        var result = await _orderService.UpdateOrderAsync(orderDto);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }
    
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatusUpdateDto statusDto)
    {
        if (id != statusDto.Id)
            return BadRequest();
        
        var result = await _orderService.UpdateOrderStatusAsync(statusDto);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var result = await _orderService.DeleteOrderAsync(id);
        
        if (!result)
            return NotFound();
        
        return NoContent();
    }
} 