# Clean Architecture .NET Core Implementation

This project demonstrates a complete implementation of Clean Architecture principles in .NET Core 8.0 with a simple Product CRUD API.

## 🏗️ Architecture Overview

Clean Architecture separates the application into distinct layers, each with specific responsibilities and dependencies flowing inward.

```
┌─────────────────────────────────────────────┐
│         Presentation Layer (API)            │
│     Controllers, DTOs, Middleware           │
└──────────────┬──────────────────────────────┘
               │
┌──────────────▼──────────────────────────────┐
│        Application Layer                    │
│   Services, Interfaces, Business Logic      │
└──────────────┬──────────────────────────────┘
               │
┌──────────────▼──────────────────────────────┐
│        Infrastructure Layer                 │
│  EF Core, Repositories, External Services   │
└──────────────┬──────────────────────────────┘
               │
┌──────────────▼──────────────────────────────┐
│          Domain Layer                       │
│     Entities, Interfaces, Domain Logic      │
└─────────────────────────────────────────────┘
```

## 📁 Project Structure

```
clean-architecture/
├── CleanArchitecture.Domain/           # Core domain entities and interfaces
│   ├── Entities/
│   │   ├── BaseEntity.cs              # Base entity with common properties
│   │   └── Product.cs                 # Product entity
│   └── Interfaces/
│       ├── IRepository.cs             # Generic repository interface
│       └── IUnitOfWork.cs             # Unit of Work pattern interface
│
├── CleanArchitecture.Application/      # Application business logic
│   ├── DTOs/
│   │   ├── ProductDto.cs              # Product data transfer object
│   │   ├── CreateProductDto.cs        # DTO for creating products
│   │   └── UpdateProductDto.cs        # DTO for updating products
│   ├── Interfaces/
│   │   └── IProductService.cs         # Product service interface
│   └── Services/
│       └── ProductService.cs          # Product service implementation
│
├── CleanArchitecture.Infrastructure/   # Data access and external services
│   ├── Data/
│   │   └── ApplicationDbContext.cs    # EF Core DbContext
│   └── Repositories/
│       ├── Repository.cs              # Generic repository implementation
│       └── UnitOfWork.cs              # Unit of Work implementation
│
└── CleanArchitecture.API/              # Presentation layer (Web API)
    ├── Controllers/
    │   └── ProductsController.cs      # Products REST API controller
    ├── Program.cs                     # Application entry point & DI setup
    ├── appsettings.json              # Application configuration
    └── appsettings.Development.json  # Development configuration
```

## 🎯 Key Principles Implemented

### 1. **Dependency Inversion**
- Inner layers define interfaces
- Outer layers implement those interfaces
- Dependencies point inward (API → Application → Domain)

### 2. **Separation of Concerns**
- **Domain**: Pure business entities and rules
- **Application**: Use cases and orchestration
- **Infrastructure**: Data access and external concerns
- **API**: HTTP concerns and presentation logic

### 3. **Repository Pattern**
- Abstracts data access logic
- Generic `IRepository<T>` for common operations
- Easily swappable data sources

### 4. **Unit of Work Pattern**
- Manages transactions across repositories
- Ensures data consistency

### 5. **Dependency Injection**
- All dependencies injected via constructor
- Configured in `Program.cs`

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- Your favorite IDE (Visual Studio, VS Code, Rider)

### Running the Application

1. **Navigate to the API project:**
   ```bash
   cd CleanArchitecture.API
   ```

2. **Run the application:**
   ```bash
   dotnet run
   ```

3. **Access Swagger UI:**
   - Open your browser to `https://localhost:5001` or `http://localhost:5000`
   - Swagger UI will load automatically at the root URL

## 📝 API Endpoints

### Products API

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Get all products |
| GET | `/api/products/{id}` | Get product by ID |
| POST | `/api/products` | Create a new product |
| PUT | `/api/products/{id}` | Update an existing product |
| DELETE | `/api/products/{id}` | Delete a product |

### Example Requests

**Create Product:**
```json
POST /api/products
{
  "name": "Gaming Laptop",
  "description": "High-performance gaming laptop",
  "price": 1299.99,
  "stock": 25,
  "isActive": true
}
```

**Update Product:**
```json
PUT /api/products/1
{
  "name": "Gaming Laptop Pro",
  "description": "Updated high-performance gaming laptop",
  "price": 1399.99,
  "stock": 20,
  "isActive": true
}
```

## 🔧 Configuration

### Database
Currently configured to use **InMemory Database** for demonstration purposes.

To switch to SQL Server:

1. Update `Program.cs`:
   ```csharp
   builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   ```

2. Update connection string in `appsettings.json`

3. Run migrations:
   ```bash
   dotnet ef migrations add InitialCreate --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.API
   dotnet ef database update --project CleanArchitecture.Infrastructure --startup-project CleanArchitecture.API
   ```

## 🧪 Testing the API

### Using Swagger UI
1. Navigate to the root URL when the app is running
2. Try the endpoints interactively

### Using curl

**Get all products:**
```bash
curl -X GET http://localhost:5000/api/products
```

**Get product by ID:**
```bash
curl -X GET http://localhost:5000/api/products/1
```

**Create a product:**
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "New Product",
    "description": "Product description",
    "price": 99.99,
    "stock": 50,
    "isActive": true
  }'
```

**Update a product:**
```bash
curl -X PUT http://localhost:5000/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Product",
    "description": "Updated description",
    "price": 89.99,
    "stock": 45,
    "isActive": true
  }'
```

**Delete a product:**
```bash
curl -X DELETE http://localhost:5000/api/products/1
```

## 📦 Dependencies

### Domain Layer
- No external dependencies (pure C#)

### Application Layer
- `CleanArchitecture.Domain` project reference

### Infrastructure Layer
- `Microsoft.EntityFrameworkCore` (8.0.0)
- `Microsoft.EntityFrameworkCore.InMemory` (8.0.0)
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.0)
- `CleanArchitecture.Application` project reference
- `CleanArchitecture.Domain` project reference

### API Layer
- `Swashbuckle.AspNetCore` (6.5.0) - Swagger/OpenAPI
- `CleanArchitecture.Application` project reference
- `CleanArchitecture.Infrastructure` project reference

## 🎓 Learning Resources

### Clean Architecture Concepts
- **Domain Layer**: Contains business entities and domain logic. No dependencies on other layers.
- **Application Layer**: Contains business logic, use cases, and orchestrates domain objects.
- **Infrastructure Layer**: Implements interfaces defined in Application/Domain. Handles data persistence, external APIs, etc.
- **Presentation Layer**: Handles HTTP requests/responses, input validation, and API documentation.

### Design Patterns Used
- **Repository Pattern**: Abstracts data access
- **Unit of Work Pattern**: Manages transactions
- **Dependency Injection**: Loose coupling between components
- **DTO Pattern**: Data transfer between layers

## 🔍 Code Flow Example

When a client creates a product via POST `/api/products`:

1. **API Layer** (`ProductsController.Create`)
   - Receives HTTP request with `CreateProductDto`
   - Validates the request
   - Calls `IProductService.CreateAsync`

2. **Application Layer** (`ProductService.CreateAsync`)
   - Maps DTO to Domain entity
   - Calls repository to add the entity
   - Saves changes via Unit of Work
   - Maps entity back to DTO
   - Returns DTO

3. **Infrastructure Layer** (`Repository<Product>.AddAsync`)
   - Adds entity to DbContext
   - Actual save happens in UnitOfWork

4. **Domain Layer** (`Product` entity)
   - Contains the business rules and properties
   - No dependency on frameworks

## 🚀 Future Enhancements

- Add FluentValidation for DTO validation
- Implement CQRS with MediatR
- Add AutoMapper for object mapping
- Implement caching strategy
- Add authentication & authorization
- Add unit and integration tests
- Add API versioning
- Implement global error handling middleware
- Add logging with Serilog

## 📄 License

This is a demonstration project for educational purposes.
