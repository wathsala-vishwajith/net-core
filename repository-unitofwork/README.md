# Repository and Unit of Work Pattern Implementation

A comprehensive .NET Core 8 application demonstrating the **Repository** and **Unit of Work** design patterns with both **SQL (Entity Framework Core)** and **NoSQL (MongoDB)** implementations.

## 📋 Table of Contents

- [Overview](#overview)
- [Project Structure](#project-structure)
- [Design Patterns](#design-patterns)
- [Technologies](#technologies)
- [Getting Started](#getting-started)
- [API Endpoints](#api-endpoints)
- [Key Features](#key-features)
- [Examples](#examples)

## 🎯 Overview

This project demonstrates enterprise-level data access patterns used in modern .NET applications. It showcases how to implement a clean, maintainable architecture that supports multiple data sources while maintaining a consistent API.

### What You'll Learn

- **Repository Pattern**: Abstraction layer between business logic and data access
- **Unit of Work Pattern**: Managing transactions across multiple repositories
- **Specification Pattern**: Encapsulating query logic
- **Dual Database Support**: Same interfaces, different implementations (SQL & NoSQL)
- **Clean Architecture**: Separation of concerns with proper layering
- **Dependency Injection**: Proper DI setup for both implementations

## 📁 Project Structure

```
repository-unitofwork/
├── RepositoryUoW.Domain/              # Domain entities and models
│   ├── Common/
│   │   └── BaseEntity.cs              # Base entity with common properties
│   └── Entities/
│       ├── Product.cs
│       ├── Customer.cs
│       ├── Order.cs
│       └── OrderItem.cs
│
├── RepositoryUoW.Core/                # Core abstractions and interfaces
│   ├── Interfaces/
│   │   ├── IRepository.cs             # Generic repository interface
│   │   └── IUnitOfWork.cs             # Unit of Work interface
│   └── Specifications/
│       ├── ISpecification.cs          # Specification pattern interface
│       ├── BaseSpecification.cs       # Base specification implementation
│       ├── ProductSpecifications.cs   # Product-specific specifications
│       └── OrderSpecifications.cs     # Order-specific specifications
│
├── RepositoryUoW.Infrastructure.SQL/  # SQL/EF Core implementation
│   ├── Data/
│   │   └── ApplicationDbContext.cs    # EF Core DbContext
│   ├── Configurations/                # Entity configurations
│   │   ├── ProductConfiguration.cs
│   │   ├── CustomerConfiguration.cs
│   │   ├── OrderConfiguration.cs
│   │   └── OrderItemConfiguration.cs
│   ├── Repositories/
│   │   └── EfRepository.cs            # EF Core repository implementation
│   └── UnitOfWork/
│       └── EfUnitOfWork.cs            # EF Core Unit of Work
│
├── RepositoryUoW.Infrastructure.MongoDB/  # MongoDB implementation
│   ├── Settings/
│   │   └── MongoDbSettings.cs         # MongoDB configuration
│   ├── Data/
│   │   └── MongoDbContext.cs          # MongoDB context
│   ├── Repositories/
│   │   └── MongoRepository.cs         # MongoDB repository implementation
│   └── UnitOfWork/
│       └── MongoUnitOfWork.cs         # MongoDB Unit of Work
│
└── RepositoryUoW.API/                 # Web API project
    ├── Controllers/
    │   ├── SqlProductsController.cs   # SQL implementation endpoints
    │   ├── SqlCustomersController.cs
    │   └── MongoProductsController.cs # MongoDB implementation endpoints
    ├── DTOs/                          # Data Transfer Objects
    │   ├── ProductDto.cs
    │   └── CustomerDto.cs
    ├── Program.cs                     # Application configuration
    └── appsettings.json               # Configuration settings
```

## 🏗️ Design Patterns

### 1. Repository Pattern

The Repository pattern provides an abstraction layer between the business logic and data access logic. It encapsulates the logic required to access data sources.

**Benefits:**
- Centralizes data access logic
- Provides a substitution point for unit testing (mock repositories)
- Minimizes duplicate query logic
- Decouples application from persistence frameworks

**Interface:**
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, ...);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    // ... more methods
}
```

### 2. Unit of Work Pattern

The Unit of Work pattern maintains a list of objects affected by a business transaction and coordinates the writing out of changes.

**Benefits:**
- Ensures data consistency with transactions
- Reduces database round trips
- Maintains a single instance of each entity in memory
- Coordinates changes across multiple repositories

**Interface:**
```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<Product> Products { get; }
    IRepository<Customer> Customers { get; }
    IRepository<Order> Orders { get; }
    IRepository<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

### 3. Specification Pattern

The Specification pattern encapsulates query logic into reusable, composable objects.

**Example:**
```csharp
public class ProductsByCategorySpecification : BaseSpecification<Product>
{
    public ProductsByCategorySpecification(string category)
        : base(p => p.Category == category && !p.IsDeleted)
    {
        ApplyOrderBy(p => p.Name);
    }
}
```

## 🛠️ Technologies

- **.NET 8.0**: Latest .NET framework
- **Entity Framework Core 8.0**: ORM for SQL databases
- **MongoDB.Driver 2.23**: MongoDB official driver
- **ASP.NET Core**: Web API framework
- **Swagger/OpenAPI**: API documentation
- **In-Memory Database**: For SQL demo (easily switchable to SQL Server)

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- MongoDB (optional - for MongoDB implementation testing)
- Visual Studio 2022 / VS Code / Rider

### Installation

1. **Clone the repository:**
```bash
cd /home/user/net-core/repository-unitofwork
```

2. **Restore dependencies:**
```bash
dotnet restore
```

3. **Run the application:**
```bash
cd RepositoryUoW.API
dotnet run
```

4. **Access Swagger UI:**
Open your browser and navigate to: `https://localhost:5001` or `http://localhost:5000`

### Configuration

Edit `appsettings.json` to configure database connections:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RepositoryUoWDb;..."
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "RepositoryUoWDb"
  }
}
```

## 📡 API Endpoints

### SQL Implementation (Entity Framework Core)

#### Products
- `GET /api/sql/products` - Get all products
- `GET /api/sql/products/{id}` - Get product by ID
- `GET /api/sql/products/category/{category}` - Get products by category
- `GET /api/sql/products/paged?page=1&pageSize=10` - Get paginated products
- `POST /api/sql/products` - Create a new product
- `PUT /api/sql/products/{id}` - Update a product
- `DELETE /api/sql/products/{id}` - Delete a product (hard delete)
- `DELETE /api/sql/products/{id}/soft` - Soft delete a product
- `POST /api/sql/products/transaction-demo` - Demonstrate transaction rollback

#### Customers
- `GET /api/sql/customers` - Get all customers
- `GET /api/sql/customers/{id}` - Get customer by ID
- `GET /api/sql/customers/search?email={email}` - Search by email
- `POST /api/sql/customers` - Create a new customer
- `PUT /api/sql/customers/{id}` - Update a customer
- `DELETE /api/sql/customers/{id}` - Delete a customer

### MongoDB Implementation

#### Products
- `GET /api/mongo/products` - Get all products
- `GET /api/mongo/products/{id}` - Get product by ID
- `GET /api/mongo/products/category/{category}` - Get products by category
- `GET /api/mongo/products/paged?page=1&pageSize=10` - Get paginated products
- `POST /api/mongo/products` - Create a new product
- `PUT /api/mongo/products/{id}` - Update a product
- `DELETE /api/mongo/products/{id}` - Delete a product
- `DELETE /api/mongo/products/{id}/soft` - Soft delete a product
- `POST /api/mongo/products/transaction-demo` - Demonstrate MongoDB transaction

## ✨ Key Features

### 1. Dual Database Support

The same business logic works with both SQL and MongoDB:

```csharp
// SQL implementation
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();

// MongoDB implementation
builder.Services.AddScoped<MongoUnitOfWork>();
```

### 2. Soft Delete

Both implementations support soft delete functionality:

```csharp
await unitOfWork.Products.SoftDeleteAsync(productId);
```

### 3. Pagination

Built-in pagination support:

```csharp
var (items, totalCount) = await unitOfWork.Products.GetPagedAsync(
    pageNumber: 1,
    pageSize: 10,
    filter: p => p.Category == "Electronics",
    orderBy: q => q.OrderBy(p => p.Name)
);
```

### 4. Transaction Management

Full transaction support for both SQL and MongoDB:

```csharp
await unitOfWork.BeginTransactionAsync();
try
{
    await unitOfWork.Products.AddAsync(product1);
    await unitOfWork.Products.AddAsync(product2);
    await unitOfWork.CommitTransactionAsync();
}
catch
{
    await unitOfWork.RollbackTransactionAsync();
}
```

### 5. Specification Pattern

Reusable query specifications:

```csharp
var spec = new ProductsByCategorySpecification("Electronics");
var products = await repository.FindAsync(spec.Criteria);
```

### 6. Automatic Timestamp Management

Entities automatically track creation and update times:

```csharp
public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

## 📝 Examples

### Creating a Product (SQL)

**Request:**
```bash
POST /api/sql/products
Content-Type: application/json

{
  "name": "Gaming Laptop",
  "description": "High-performance gaming laptop",
  "price": 1999.99,
  "stockQuantity": 25,
  "category": "Electronics",
  "sku": "LAP-GAME-001"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Gaming Laptop",
  "description": "High-performance gaming laptop",
  "price": 1999.99,
  "stockQuantity": 25,
  "category": "Electronics",
  "sku": "LAP-GAME-001",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null
}
```

### Using Pagination

**Request:**
```bash
GET /api/sql/products/paged?page=1&pageSize=5
```

**Response:**
```json
{
  "totalCount": 23,
  "page": 1,
  "pageSize": 5,
  "totalPages": 5,
  "items": [...]
}
```

### Transaction Example

```csharp
[HttpPost("bulk-create")]
public async Task<ActionResult> BulkCreate([FromBody] List<CreateProductDto> products)
{
    await _unitOfWork.BeginTransactionAsync();

    try
    {
        foreach (var dto in products)
        {
            var product = MapToEntity(dto);
            await _unitOfWork.Products.AddAsync(product);
        }

        await _unitOfWork.CommitTransactionAsync();
        return Ok(new { message = "Products created successfully" });
    }
    catch (Exception ex)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return BadRequest(new { message = ex.Message });
    }
}
```

## 🎓 Learning Points

### When to Use Repository Pattern

✅ **Use when:**
- You need to abstract data access logic
- You want to switch between different data sources
- You need to write unit tests with mocked data
- You have complex query logic to encapsulate

❌ **Don't use when:**
- Your application is very simple (CRUD only)
- You're using a framework that already provides good abstraction (like EF Core for simple cases)
- Over-abstraction would add unnecessary complexity

### Unit of Work vs DbContext

**Unit of Work:**
- Coordinates multiple repositories
- Manages transactions explicitly
- Provides centralized SaveChanges
- Better for complex business transactions

**DbContext alone:**
- Simpler for basic CRUD operations
- Less abstraction overhead
- Direct EF Core usage
- Good for simple applications

## 🔍 Comparison: SQL vs MongoDB Implementation

| Feature | SQL (EF Core) | MongoDB |
|---------|---------------|---------|
| **Transactions** | ACID compliant | Multi-document transactions (Replica Set required) |
| **Relationships** | Foreign keys, navigation properties | Embedded or referenced documents |
| **Querying** | LINQ to SQL | LINQ to MongoDB / Filter builders |
| **Schema** | Fixed schema with migrations | Flexible schema |
| **SaveChanges** | Explicit call required | Changes saved immediately |
| **Performance** | Optimized for relational queries | Optimized for document retrieval |

## 🤝 Contributing

This is a demonstration project for learning purposes. Feel free to:
- Add more repositories
- Implement additional patterns
- Add more database providers
- Improve existing implementations

## 📄 License

This project is for educational purposes.

## 📚 Additional Resources

- [Repository Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/repository.html)
- [Unit of Work Pattern](https://martinfowler.com/eaaCatalog/unitOfWork.html)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [MongoDB .NET Driver Documentation](https://mongodb.github.io/mongo-csharp-driver/)

---

**Built with ❤️ using .NET 8 and Clean Architecture principles**
