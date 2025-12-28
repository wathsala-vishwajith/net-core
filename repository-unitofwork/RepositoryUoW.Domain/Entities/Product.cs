using RepositoryUoW.Domain.Common;

namespace RepositoryUoW.Domain.Entities;

/// <summary>
/// Product entity representing items in inventory
/// </summary>
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
