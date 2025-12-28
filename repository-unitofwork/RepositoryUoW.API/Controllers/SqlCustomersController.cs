using Microsoft.AspNetCore.Mvc;
using RepositoryUoW.API.DTOs;
using RepositoryUoW.Core.Interfaces;
using RepositoryUoW.Domain.Entities;

namespace RepositoryUoW.API.Controllers;

/// <summary>
/// Customers controller using SQL (EF Core) implementation
/// </summary>
[ApiController]
[Route("api/sql/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(IUnitOfWork unitOfWork, ILogger<CustomersController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetAll()
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        var response = customers.Select(MapToDto);
        return Ok(response);
    }

    /// <summary>
    /// Get customer by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> GetById(Guid id)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        return Ok(MapToDto(customer));
    }

    /// <summary>
    /// Search customers by email
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<CustomerResponseDto>> SearchByEmail([FromQuery] string email)
    {
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Email == email);
        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        return Ok(MapToDto(customer));
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CustomerResponseDto>> Create([FromBody] CreateCustomerDto dto)
    {
        // Check if email already exists
        var exists = await _unitOfWork.Customers.ExistsAsync(c => c.Email == dto.Email);
        if (exists)
            return Conflict(new { message = "Customer with this email already exists" });

        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country
        };

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created customer {CustomerId} with email {Email}", customer.Id, customer.Email);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, MapToDto(customer));
    }

    /// <summary>
    /// Update a customer
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerResponseDto>> Update(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);
        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        if (dto.FirstName != null) customer.FirstName = dto.FirstName;
        if (dto.LastName != null) customer.LastName = dto.LastName;
        if (dto.Phone != null) customer.Phone = dto.Phone;
        if (dto.Address != null) customer.Address = dto.Address;
        if (dto.City != null) customer.City = dto.City;
        if (dto.Country != null) customer.Country = dto.Country;

        await _unitOfWork.Customers.UpdateAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated customer {CustomerId}", customer.Id);

        return Ok(MapToDto(customer));
    }

    /// <summary>
    /// Delete a customer
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _unitOfWork.Customers.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted customer {CustomerId}", id);

        return NoContent();
    }

    private static CustomerResponseDto MapToDto(Customer customer)
    {
        return new CustomerResponseDto
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            City = customer.City,
            Country = customer.Country,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}
