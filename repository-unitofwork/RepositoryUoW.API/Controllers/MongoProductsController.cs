using Microsoft.AspNetCore.Mvc;
using RepositoryUoW.API.DTOs;
using RepositoryUoW.Domain.Entities;
using RepositoryUoW.Infrastructure.MongoDB.UnitOfWork;

namespace RepositoryUoW.API.Controllers;

/// <summary>
/// Products controller using MongoDB implementation
/// </summary>
[ApiController]
[Route("api/mongo/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly MongoUnitOfWork _unitOfWork;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(MongoUnitOfWork unitOfWork, ILogger<ProductsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all products from MongoDB
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        var response = products.Select(MapToDto);
        return Ok(response);
    }

    /// <summary>
    /// Get product by ID from MongoDB
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(MapToDto(product));
    }

    /// <summary>
    /// Get products by category from MongoDB
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetByCategory(string category)
    {
        var products = await _unitOfWork.Products.FindAsync(p => p.Category == category);
        var response = products.Select(MapToDto);
        return Ok(response);
    }

    /// <summary>
    /// Get paginated products from MongoDB
    /// </summary>
    [HttpGet("paged")]
    public async Task<ActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(page, pageSize);

        var response = new
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Items = items.Select(MapToDto)
        };

        return Ok(response);
    }

    /// <summary>
    /// Create a new product in MongoDB
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create([FromBody] CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            Category = dto.Category,
            SKU = dto.SKU
        };

        await _unitOfWork.Products.AddAsync(product);

        _logger.LogInformation("Created product in MongoDB {ProductId} with SKU {SKU}", product.Id, product.SKU);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, MapToDto(product));
    }

    /// <summary>
    /// Update a product in MongoDB
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductResponseDto>> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Product not found" });

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.StockQuantity.HasValue) product.StockQuantity = dto.StockQuantity.Value;
        if (dto.Category != null) product.Category = dto.Category;

        await _unitOfWork.Products.UpdateAsync(product);

        _logger.LogInformation("Updated product in MongoDB {ProductId}", product.Id);

        return Ok(MapToDto(product));
    }

    /// <summary>
    /// Delete a product from MongoDB (hard delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _unitOfWork.Products.DeleteAsync(id);

        _logger.LogInformation("Deleted product from MongoDB {ProductId}", id);

        return NoContent();
    }

    /// <summary>
    /// Soft delete a product in MongoDB
    /// </summary>
    [HttpDelete("{id}/soft")]
    public async Task<ActionResult> SoftDelete(Guid id)
    {
        await _unitOfWork.Products.SoftDeleteAsync(id);

        _logger.LogInformation("Soft deleted product in MongoDB {ProductId}", id);

        return NoContent();
    }

    /// <summary>
    /// Demonstrates MongoDB transaction
    /// </summary>
    [HttpPost("transaction-demo")]
    public async Task<ActionResult> TransactionDemo()
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Create first product
            var product1 = new Product
            {
                Name = "MongoDB Transaction Test 1",
                Description = "This will be committed",
                Price = 99.99m,
                StockQuantity = 10,
                Category = "Test",
                SKU = $"MONGO-TXN-{Guid.NewGuid().ToString()[..8]}"
            };
            await _unitOfWork.Products.AddAsync(product1);

            // Create second product
            var product2 = new Product
            {
                Name = "MongoDB Transaction Test 2",
                Description = "This will also be committed",
                Price = 199.99m,
                StockQuantity = 5,
                Category = "Test",
                SKU = $"MONGO-TXN-{Guid.NewGuid().ToString()[..8]}"
            };
            await _unitOfWork.Products.AddAsync(product2);

            // Commit the transaction
            await _unitOfWork.CommitTransactionAsync();

            return Ok(new
            {
                message = "Transaction committed successfully",
                products = new[] { product1.Id, product2.Id }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MongoDB transaction demo");
            await _unitOfWork.RollbackTransactionAsync();
            return BadRequest(new { message = ex.Message });
        }
    }

    private static ProductResponseDto MapToDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category,
            SKU = product.SKU,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
