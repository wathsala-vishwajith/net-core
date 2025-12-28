using RepositoryUoW.Domain.Common;

namespace RepositoryUoW.Domain.Entities;

/// <summary>
/// OrderItem entity representing individual items within an order
/// </summary>
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }

    // Navigation properties
    public Order? Order { get; set; }
    public Product? Product { get; set; }

    public decimal TotalPrice => (UnitPrice * Quantity) - Discount;
}
