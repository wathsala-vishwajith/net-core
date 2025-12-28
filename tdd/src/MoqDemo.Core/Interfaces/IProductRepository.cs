using MoqDemo.Core.Entities;

namespace MoqDemo.Core.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetAvailableProductsAsync();
    Task<IEnumerable<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<bool> UpdateStockAsync(int productId, int quantity);
}
