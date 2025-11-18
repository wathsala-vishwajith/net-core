namespace CodeFirstMigrations.Models;

/// <summary>
/// Product entity - Initial version
/// This model will evolve through migrations
/// </summary>
public class Product
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    // Added in Migration 2
    public string? Description { get; set; }
    public string? Category { get; set; }

    // Added in Migration 3
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property - Added in Migration 4
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
}
