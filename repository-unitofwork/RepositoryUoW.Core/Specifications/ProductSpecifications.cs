using RepositoryUoW.Domain.Entities;

namespace RepositoryUoW.Core.Specifications;

/// <summary>
/// Specification for products by category
/// </summary>
public class ProductsByCategorySpecification : BaseSpecification<Product>
{
    public ProductsByCategorySpecification(string category)
        : base(p => p.Category == category && !p.IsDeleted)
    {
        ApplyOrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification for products in stock
/// </summary>
public class ProductsInStockSpecification : BaseSpecification<Product>
{
    public ProductsInStockSpecification()
        : base(p => p.StockQuantity > 0 && !p.IsDeleted)
    {
        ApplyOrderByDescending(p => p.StockQuantity);
    }
}

/// <summary>
/// Specification for products by price range
/// </summary>
public class ProductsByPriceRangeSpecification : BaseSpecification<Product>
{
    public ProductsByPriceRangeSpecification(decimal minPrice, decimal maxPrice)
        : base(p => p.Price >= minPrice && p.Price <= maxPrice && !p.IsDeleted)
    {
        ApplyOrderBy(p => p.Price);
    }
}
