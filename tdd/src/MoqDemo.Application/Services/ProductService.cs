using Microsoft.Extensions.Logging;
using MoqDemo.Core.Entities;
using MoqDemo.Core.Interfaces;

namespace MoqDemo.Application.Services;

public class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        _logger.LogInformation("Fetching product: {ProductId}", id);
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Product>> GetAvailableProductsAsync()
    {
        _logger.LogInformation("Fetching available products");
        return await _productRepository.GetAvailableProductsAsync();
    }

    public async Task<Product> CreateProductAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (product.Price <= 0)
            throw new ArgumentException("Price must be greater than zero", nameof(product));

        _logger.LogInformation("Creating product: {ProductName}", product.Name);

        product.CreatedAt = DateTime.UtcNow;
        product.IsAvailable = product.StockQuantity > 0;

        var createdProduct = await _productRepository.AddAsync(product);

        _logger.LogInformation("Product created: {ProductId}", createdProduct.Id);
        return createdProduct;
    }

    public async Task<bool> UpdateStockAsync(int productId, int quantity)
    {
        _logger.LogInformation("Updating stock for product: {ProductId}", productId);

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", productId);
            return false;
        }

        var updated = await _productRepository.UpdateStockAsync(productId, quantity);

        if (updated)
        {
            _logger.LogInformation("Stock updated for product: {ProductId}", productId);
        }

        return updated;
    }

    public async Task<IEnumerable<Product>> SearchByPriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        if (minPrice < 0 || maxPrice < 0)
            throw new ArgumentException("Prices cannot be negative");

        if (minPrice > maxPrice)
            throw new ArgumentException("Min price cannot be greater than max price");

        _logger.LogInformation("Searching products by price range: {MinPrice} - {MaxPrice}", minPrice, maxPrice);

        return await _productRepository.GetProductsByPriceRangeAsync(minPrice, maxPrice);
    }
}
