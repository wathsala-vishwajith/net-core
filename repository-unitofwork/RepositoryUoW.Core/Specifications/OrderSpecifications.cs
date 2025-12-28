using RepositoryUoW.Domain.Entities;

namespace RepositoryUoW.Core.Specifications;

/// <summary>
/// Specification for orders by customer
/// </summary>
public class OrdersByCustomerSpecification : BaseSpecification<Order>
{
    public OrdersByCustomerSpecification(Guid customerId)
        : base(o => o.CustomerId == customerId && !o.IsDeleted)
    {
        AddInclude(o => o.Customer!);
        AddInclude(o => o.OrderItems);
        ApplyOrderByDescending(o => o.OrderDate);
    }
}

/// <summary>
/// Specification for orders by status
/// </summary>
public class OrdersByStatusSpecification : BaseSpecification<Order>
{
    public OrdersByStatusSpecification(OrderStatus status)
        : base(o => o.Status == status && !o.IsDeleted)
    {
        AddInclude(o => o.Customer!);
        AddInclude(o => o.OrderItems);
        ApplyOrderByDescending(o => o.OrderDate);
    }
}

/// <summary>
/// Specification for recent orders
/// </summary>
public class RecentOrdersSpecification : BaseSpecification<Order>
{
    public RecentOrdersSpecification(int days = 30)
        : base(o => o.OrderDate >= DateTime.UtcNow.AddDays(-days) && !o.IsDeleted)
    {
        AddInclude(o => o.Customer!);
        ApplyOrderByDescending(o => o.OrderDate);
    }
}
