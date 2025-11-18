# Entity Framework Core & ASP.NET Core Examples

A comprehensive collection of example projects demonstrating **Entity Framework Core** and **ASP.NET Core** concepts, from fundamentals to advanced topics.

## 📚 Project Overview

This repository contains 7 complete example projects covering essential .NET Core topics:

| # | Project | Topics | Key Features |
|---|---------|--------|--------------|
| **01** | [**EF Core Basics**](./01-EFCore-Basics/) | Entity Framework Core fundamentals | DbContext, Entities, CRUD, Relationships, Querying |
| **02** | [**Code First Migrations**](./02-CodeFirst-Migrations/) | Database schema evolution | Migrations, Schema updates, Data seeding |
| **03** | [**Change Tracker API**](./03-ChangeTracker-API/) | Entity state management | Entity states, Change detection, Tracking |
| **04** | [**Loading Strategies**](./04-Loading-Strategies/) | Loading related data | Eager, Lazy, Explicit loading, N+1 problem |
| **05** | [**Razor Pages**](./05-RazorPages/) | Server-side web UI | PageModel, Tag Helpers, Validation, CRUD |
| **06** | [**Middlewares**](./06-Middlewares/) | Request pipeline | Custom middleware, Pipeline order, Filters |
| **07** | [**Minimal APIs**](./07-Minimal-APIs/) | Modern API development | Endpoints, OpenAPI, Swagger, REST API |

## 🎯 Learning Path

### Beginner Path
Start here if you're new to .NET Core:

1. **[01-EFCore-Basics](./01-EFCore-Basics/)** - Learn Entity Framework Core fundamentals
   - What is EF Core and how it works
   - Creating DbContext and entities
   - Basic CRUD operations
   - Simple querying with LINQ

2. **[02-CodeFirst-Migrations](./02-CodeFirst-Migrations/)** - Understand database evolution
   - Code First approach
   - Creating and applying migrations
   - Updating database schema
   - Handling schema changes

3. **[05-RazorPages](./05-RazorPages/)** - Build your first web application
   - Page-based web development
   - Forms and validation
   - Building CRUD interfaces
   - Working with data in web apps

### Intermediate Path
Continue with these after mastering the basics:

4. **[03-ChangeTracker-API](./03-ChangeTracker-API/)** - Deep dive into change tracking
   - Understanding entity states
   - How EF Core tracks changes
   - Performance optimization
   - Working with detached entities

5. **[04-Loading-Strategies](./04-Loading-Strategies/)** - Master data loading
   - Eager vs Lazy vs Explicit loading
   - Avoiding N+1 query problems
   - Performance considerations
   - Choosing the right strategy

6. **[06-Middlewares](./06-Middlewares/)** - Understand the request pipeline
   - How middleware works
   - Creating custom middleware
   - Pipeline ordering
   - Authentication, logging, error handling

### Advanced Path
Explore modern API development:

7. **[07-Minimal-APIs](./07-Minimal-APIs/)** - Build modern APIs
   - Minimal API approach
   - RESTful API design
   - OpenAPI/Swagger documentation
   - Dependency injection in endpoints

## 🚀 Quick Start

### Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) (or later)
- Code editor ([VS Code](https://code.visualstudio.com/), [Visual Studio](https://visualstudio.microsoft.com/), or [Rider](https://www.jetbrains.com/rider/))
- Basic C# knowledge

### Running a Project

1. **Navigate to a project folder:**
   ```bash
   cd entity-framework/01-EFCore-Basics
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the project:**
   ```bash
   dotnet run
   ```

   For web projects (05, 06, 07), open browser to the displayed URL (usually `http://localhost:5000`).

### Running with Hot Reload

For web projects, use watch mode:
```bash
dotnet watch run
```

## 📖 Project Details

### 01 - EF Core Basics
**Technologies:** EF Core, SQLite, LINQ

Learn the fundamentals of Entity Framework Core:
- DbContext configuration
- Entity modeling and relationships
- CRUD operations (Create, Read, Update, Delete)
- Basic querying with LINQ
- Navigation properties
- Data seeding

**Key Concepts:**
- One-to-Many relationships
- Many-to-Many relationships
- Primary and foreign keys
- Fluent API configuration
- Database initialization

[➡️ View Project](./01-EFCore-Basics/)

---

### 02 - Code First Migrations
**Technologies:** EF Core Migrations, SQLite

Master database schema evolution:
- Creating initial migrations
- Updating database schema
- Adding/removing entities
- Modifying properties
- Rolling back changes
- Production deployment strategies

**Key Concepts:**
- `dotnet ef migrations add`
- `dotnet ef database update`
- Migration Up() and Down() methods
- Model snapshots
- Data migration strategies

[➡️ View Project](./02-CodeFirst-Migrations/)

---

### 03 - Change Tracker API
**Technologies:** EF Core Change Tracking

Understand how EF Core tracks entity changes:
- Entity states (Detached, Unchanged, Added, Modified, Deleted)
- Change detection mechanisms
- Original vs Current values
- No-tracking queries
- Performance optimization

**Key Concepts:**
- `ChangeTracker.Entries()`
- `EntityState`
- `DetectChanges()`
- `AsNoTracking()`
- Manual state management

[➡️ View Project](./03-ChangeTracker-API/)

---

### 04 - Loading Strategies
**Technologies:** EF Core, Lazy Loading Proxies

Learn different ways to load related data:
- **Eager Loading** - Load everything upfront with `Include()`
- **Lazy Loading** - Load on-demand automatically
- **Explicit Loading** - Load manually when needed
- **Select/Projection** - Load only specific fields

**Key Concepts:**
- `Include()` and `ThenInclude()`
- `UseLazyLoadingProxies()`
- `Entry().Collection().Load()`
- N+1 query problem
- Performance comparison

[➡️ View Project](./04-Loading-Strategies/)

---

### 05 - Razor Pages
**Technologies:** ASP.NET Core Razor Pages, EF Core, Bootstrap

Build a complete web application:
- Movie management CRUD interface
- Server-side rendering
- Form handling and validation
- Search and filtering
- Tag Helpers
- PageModel pattern

**Key Concepts:**
- `@page` directive
- PageModel handlers (OnGet, OnPost)
- `[BindProperty]` attribute
- Data annotations
- Tag Helpers (asp-for, asp-page)
- Layout and partial views

[➡️ View Project](./05-RazorPages/)

---

### 06 - Middlewares
**Technologies:** ASP.NET Core Middleware

Understand the request pipeline:
- Custom middleware components
- Request/response logging
- Authentication middleware
- Exception handling
- Response time measurement
- Pipeline ordering

**Key Concepts:**
- `RequestDelegate`
- `InvokeAsync()`
- Middleware ordering
- `app.Use()` vs `app.Run()`
- Extension methods
- Conditional middleware

[➡️ View Project](./06-Middlewares/)

---

### 07 - Minimal APIs
**Technologies:** ASP.NET Core Minimal APIs, OpenAPI/Swagger

Build modern HTTP APIs:
- Complete Todo CRUD API
- RESTful endpoint design
- Route parameters and query strings
- OpenAPI/Swagger documentation
- Dependency injection
- Endpoint organization

**Key Concepts:**
- `MapGet()`, `MapPost()`, `MapPut()`, `MapDelete()`
- `Results` helpers
- Route constraints
- `MapGroup()` for organization
- `WithOpenApi()` metadata
- DTOs and validation

[➡️ View Project](./07-Minimal-APIs/)

## 🛠️ Common Commands

### Entity Framework Core

```bash
# Add a migration
dotnet ef migrations add <MigrationName>

# Update database
dotnet ef database update

# List migrations
dotnet ef migrations list

# Remove last migration (if not applied)
dotnet ef migrations remove

# Drop database
dotnet ef database drop

# Generate SQL script
dotnet ef migrations script
```

### .NET CLI

```bash
# Restore packages
dotnet restore

# Build project
dotnet build

# Run project
dotnet run

# Run with hot reload
dotnet watch run

# Clean build artifacts
dotnet clean
```

## 📦 NuGet Packages Used

### Entity Framework Core
- `Microsoft.EntityFrameworkCore` - Core EF functionality
- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite database provider
- `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server provider
- `Microsoft.EntityFrameworkCore.Design` - Design-time tools
- `Microsoft.EntityFrameworkCore.Proxies` - Lazy loading proxies

### ASP.NET Core
- `Microsoft.AspNetCore.OpenApi` - OpenAPI support
- `Swashbuckle.AspNetCore` - Swagger UI
- `Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore` - EF Core error pages

## 🎓 Key Concepts Covered

### Entity Framework Core
- ✅ DbContext and DbSet
- ✅ Entity configuration (Data Annotations & Fluent API)
- ✅ Relationships (One-to-One, One-to-Many, Many-to-Many)
- ✅ CRUD operations
- ✅ LINQ queries
- ✅ Migrations and schema evolution
- ✅ Change tracking and entity states
- ✅ Loading strategies (Eager, Lazy, Explicit)
- ✅ Performance optimization

### ASP.NET Core
- ✅ Razor Pages (Page-based development)
- ✅ Middleware pipeline
- ✅ Minimal APIs
- ✅ Dependency Injection
- ✅ Routing
- ✅ Model Binding and Validation
- ✅ OpenAPI/Swagger documentation
- ✅ Tag Helpers
- ✅ Request/Response handling

## 💡 Tips and Best Practices

### Entity Framework Core

1. **Use async methods** for database operations:
   ```csharp
   // ✅ Good
   await context.Todos.ToListAsync();

   // ❌ Avoid
   context.Todos.ToList();
   ```

2. **Use AsNoTracking** for read-only queries:
   ```csharp
   var items = await context.Items
       .AsNoTracking()
       .ToListAsync();
   ```

3. **Avoid N+1 queries** with eager loading:
   ```csharp
   // ✅ Good - 1 query
   var posts = await context.Posts
       .Include(p => p.Comments)
       .ToListAsync();

   // ❌ Bad - N+1 queries
   var posts = await context.Posts.ToListAsync();
   foreach (var post in posts)
   {
       var comments = post.Comments.ToList(); // Lazy load
   }
   ```

4. **Use migrations** for schema changes:
   ```bash
   dotnet ef migrations add AddUserTable
   dotnet ef database update
   ```

### ASP.NET Core

1. **Use DTOs** instead of exposing entities:
   ```csharp
   // ✅ Good
   public record TodoDto(int Id, string Title, bool IsCompleted);

   // ❌ Avoid
   return await context.Todos.ToListAsync(); // Exposes entity
   ```

2. **Validate input** before processing:
   ```csharp
   if (!ModelState.IsValid)
       return Page();
   ```

3. **Order middleware** correctly:
   ```csharp
   app.UseExceptionHandler();  // First
   app.UseAuthentication();
   app.UseAuthorization();
   app.MapControllers();       // Last
   ```

4. **Use proper HTTP status codes**:
   - `200 OK` - Success
   - `201 Created` - Resource created
   - `204 No Content` - Success with no response body
   - `400 Bad Request` - Invalid input
   - `404 Not Found` - Resource not found
   - `500 Internal Server Error` - Server error

## 📚 Additional Resources

### Official Documentation
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/)
- [C# Programming Guide](https://learn.microsoft.com/en-us/dotnet/csharp/)

### Tutorials
- [EF Core Tutorial](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app)
- [Razor Pages Tutorial](https://learn.microsoft.com/en-us/aspnet/core/tutorials/razor-pages/)
- [Minimal API Tutorial](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api)

### Tools
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Visual Studio Code](https://code.visualstudio.com/)
- [SQL Server Management Studio](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- [DB Browser for SQLite](https://sqlitebrowser.org/)

## 🤝 Contributing

These projects are designed for learning. Feel free to:
- Experiment with the code
- Add new features
- Try different database providers
- Extend the examples
- Create your own variations

## 📄 License

These examples are provided for educational purposes. Use them freely for learning and experimentation.

## 🎯 Next Steps

After completing these projects, consider:

1. **Explore other database providers:**
   - PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`)
   - MySQL (`Pomelo.EntityFrameworkCore.MySql`)
   - SQL Server (`Microsoft.EntityFrameworkCore.SqlServer`)

2. **Add authentication and authorization:**
   - ASP.NET Core Identity
   - JWT authentication
   - OAuth/OpenID Connect

3. **Implement advanced patterns:**
   - Repository pattern
   - Unit of Work
   - CQRS (Command Query Responsibility Segregation)
   - Specification pattern

4. **Add testing:**
   - Unit tests (xUnit, NUnit)
   - Integration tests
   - EF Core In-Memory provider for testing

5. **Deploy to production:**
   - Azure App Service
   - Docker containers
   - Kubernetes

## 📧 Questions?

Each project includes a detailed README with:
- Overview and key concepts
- Step-by-step examples
- Code explanations with line references
- Best practices
- Common pitfalls to avoid
- Additional learning resources

Happy coding! 🚀
