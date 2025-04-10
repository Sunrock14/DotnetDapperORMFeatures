namespace DapperSamples.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public Category? Category { get; set; }
} 