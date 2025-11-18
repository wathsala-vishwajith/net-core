# ASP.NET Core Minimal APIs

This project demonstrates **ASP.NET Core Minimal APIs**, a simplified approach to building HTTP APIs with minimal dependencies, configuration, and ceremony.

## Overview

Minimal APIs were introduced in .NET 6 and provide a streamlined way to create HTTP APIs with minimal code. They're perfect for:
- Microservices
- Small APIs
- Quick prototypes
- Learning HTTP APIs
- Cloud-native applications

## Project Structure

```
07-Minimal-APIs/
├── Data/
│   └── TodoDb.cs                  # EF Core DbContext
├── Models/
│   └── Todo.cs                    # Todo entity
├── DTOs/
│   └── TodoDto.cs                 # Data Transfer Objects
├── Endpoints/
│   └── TodoEndpoints.cs           # Endpoint definitions
├── Program.cs                     # Application entry & configuration
├── appsettings.json              # Configuration
├── MinimalAPIs.csproj            # Project file
└── README.md                      # This file
```

## Key Features

### 1. Top-Level Statements

No `Main` method, no class wrapper - just code:

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
```

**Location:** `Program.cs`

### 2. Lambda-Based Endpoints

Define endpoints using lambda expressions:

```csharp
// Simple GET
app.MapGet("/hello", () => "Hello World!");

// GET with route parameter
app.MapGet("/hello/{name}", (string name) => $"Hello, {name}!");

// POST with body
app.MapPost("/todos", (Todo todo) => Results.Created($"/todos/{todo.Id}", todo));

// With dependency injection
app.MapGet("/todos", async (TodoDb db) => await db.Todos.ToListAsync());
```

**Location:** `Program.cs:91-152`

### 3. Endpoint Groups

Organize related endpoints with `MapGroup()`:

```csharp
var todoGroup = app.MapGroup("/api/todos")
    .WithTags("Todos")
    .WithOpenApi();

todoGroup.MapGet("/", GetAllTodos);
todoGroup.MapGet("/{id}", GetTodoById);
todoGroup.MapPost("/", CreateTodo);
todoGroup.MapPut("/{id}", UpdateTodo);
todoGroup.MapDelete("/{id}", DeleteTodo);
```

**Location:** `Endpoints/TodoEndpoints.cs:15-56`

### 4. Built-in Results

Typed results for common HTTP responses:

```csharp
// 200 OK
return Results.Ok(data);

// 201 Created
return Results.Created($"/todos/{id}", todo);

// 204 No Content
return Results.NoContent();

// 400 Bad Request
return Results.BadRequest(new { error = "Invalid input" });

// 404 Not Found
return Results.NotFound(new { message = "Not found" });

// 500 Internal Server Error
return Results.Problem("Something went wrong", statusCode: 500);
```

**Location:** Throughout `Endpoints/TodoEndpoints.cs`

### 5. OpenAPI/Swagger Support

Automatic API documentation:

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Todo API",
        Description = "A minimal API for managing todos",
        Version = "v1"
    });
});

app.UseSwagger();
app.UseSwaggerUI();
```

**Location:** `Program.cs:16-29`

Access Swagger UI at: `http://localhost:5000/swagger`

## CRUD API Example

### Complete Todo API

The project includes a full CRUD API for managing todos:

**Endpoints:**

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/todos` | Get all todos (with filters) |
| GET | `/api/todos/{id}` | Get todo by ID |
| POST | `/api/todos` | Create new todo |
| PUT | `/api/todos/{id}` | Update todo |
| DELETE | `/api/todos/{id}` | Delete todo |
| GET | `/api/todos/completed` | Get completed todos |
| GET | `/api/todos/pending` | Get pending todos |
| POST | `/api/todos/{id}/complete` | Mark todo complete |
| GET | `/api/todos/stats` | Get statistics |

**Location:** `Endpoints/TodoEndpoints.cs`

### Example Usage

**Create a todo:**
```bash
curl -X POST http://localhost:5000/api/todos \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Learn Minimal APIs",
    "description": "Study ASP.NET Core Minimal APIs",
    "priority": "High",
    "category": "Learning"
  }'
```

**Get all todos:**
```bash
curl http://localhost:5000/api/todos
```

**Filter todos:**
```bash
# By category
curl "http://localhost:5000/api/todos?category=Learning"

# By priority
curl "http://localhost:5000/api/todos?priority=High"

# By completion status
curl "http://localhost:5000/api/todos?isCompleted=false"
```

**Update a todo:**
```bash
curl -X PUT http://localhost:5000/api/todos/1 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Title",
    "isCompleted": true
  }'
```

**Delete a todo:**
```bash
curl -X DELETE http://localhost:5000/api/todos/1
```

**Get statistics:**
```bash
curl http://localhost:5000/api/todos/stats
```

## Dependency Injection

Services are automatically injected into endpoint handlers:

```csharp
// Inject DbContext
app.MapGet("/todos", async (TodoDb db) =>
    await db.Todos.ToListAsync());

// Inject multiple services
app.MapGet("/example", async (
    TodoDb db,
    ILogger<Program> logger,
    HttpContext context) =>
{
    logger.LogInformation("Endpoint called");
    var todos = await db.Todos.ToListAsync();
    return Results.Ok(todos);
});
```

**Supported injections:**
- `HttpContext`
- `HttpRequest`
- `HttpResponse`
- `ClaimsPrincipal`
- `CancellationToken`
- Any registered service (e.g., `TodoDb`, `ILogger<T>`)

**Location:** Examples in `Program.cs:122-134`

## Route Parameters

### Route Constraints

```csharp
// Integer constraint
app.MapGet("/todos/{id:int}", (int id) => $"Todo ID: {id}");

// String with min length
app.MapGet("/search/{term:minlength(3)}", (string term) => $"Searching: {term}");

// Multiple constraints
app.MapGet("/items/{id:int:min(1)}", (int id) => $"Item: {id}");

// Optional parameter
app.MapGet("/greet/{name?}", (string? name) =>
    $"Hello, {name ?? "Guest"}!");
```

**Common constraints:**
- `:int` - Integer
- `:bool` - Boolean
- `:datetime` - DateTime
- `:decimal` - Decimal
- `:guid` - GUID
- `:minlength(n)` - Minimum length
- `:maxlength(n)` - Maximum length
- `:min(n)` - Minimum value
- `:max(n)` - Maximum value

## Query Parameters

Automatically bind from query string:

```csharp
app.MapGet("/search", (
    string? query,
    int page = 1,
    int pageSize = 10,
    bool includeDeleted = false) =>
{
    return new
    {
        query,
        page,
        pageSize,
        includeDeleted
    };
});

// Call: /search?query=test&page=2&pageSize=20
```

**Location:** `Endpoints/TodoEndpoints.cs:60-75`

## Request Body Binding

### JSON Body

```csharp
record CreateTodoDto(string Title, string? Description);

app.MapPost("/todos", (CreateTodoDto dto, TodoDb db) =>
{
    var todo = new Todo { Title = dto.Title };
    db.Todos.Add(todo);
    db.SaveChanges();
    return Results.Created($"/todos/{todo.Id}", todo);
});
```

### Form Data

```csharp
app.MapPost("/upload", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    // Process file
});
```

## Response Types

### Typed Responses

```csharp
app.MapGet("/todos/{id}", async (int id, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);
    return todo is not null
        ? Results.Ok(todo)
        : Results.NotFound();
})
.Produces<Todo>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);
```

### Custom Response

```csharp
app.MapGet("/custom", () =>
    Results.Json(
        new { message = "Custom" },
        statusCode: 200,
        contentType: "application/json"
    ));
```

## Metadata and Documentation

### OpenAPI Annotations

```csharp
app.MapGet("/todos", GetTodos)
    .WithName("GetAllTodos")
    .WithTags("Todos")
    .WithSummary("Retrieves all todos")
    .WithDescription("Returns a list of all todo items with optional filtering")
    .Produces<List<Todo>>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status500InternalServerError);
```

**Location:** `Endpoints/TodoEndpoints.cs:18-51`

### Grouping with Tags

```csharp
var group = app.MapGroup("/api/todos")
    .WithTags("Todos")
    .WithOpenApi();
```

## Endpoint Organization

### Extension Methods (Recommended)

Create extension methods for better organization:

```csharp
// TodoEndpoints.cs
public static class TodoEndpoints
{
    public static void MapTodoEndpoints(this WebApplication app)
    {
        app.MapGet("/todos", GetAllTodos);
        app.MapPost("/todos", CreateTodo);
        // ... more endpoints
    }

    private static async Task<IResult> GetAllTodos(TodoDb db)
    {
        var todos = await db.Todos.ToListAsync();
        return Results.Ok(todos);
    }
}

// Program.cs
app.MapTodoEndpoints();
```

**Location:** `Endpoints/TodoEndpoints.cs`

## Filters and Middleware

### Endpoint Filters

```csharp
app.MapGet("/admin", () => "Admin")
    .AddEndpointFilter(async (context, next) =>
    {
        // Before endpoint
        Console.WriteLine("Before");

        var result = await next(context);

        // After endpoint
        Console.WriteLine("After");

        return result;
    });
```

### Global Middleware

```csharp
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Path}");
    await next();
});
```

## Authentication & Authorization

```csharp
builder.Services.AddAuthentication()
    .AddJwtBearer();
builder.Services.AddAuthorization();

app.UseAuthentication();
app.UseAuthorization();

// Require authentication
app.MapGet("/secure", () => "Secure data")
    .RequireAuthorization();

// Require specific role
app.MapGet("/admin", () => "Admin only")
    .RequireAuthorization("AdminPolicy");
```

## CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors();
```

**Location:** `Program.cs:32-41`

## Running the Application

### 1. Run

```bash
dotnet run
```

Or with hot reload:

```bash
dotnet watch run
```

### 2. Access Swagger UI

Open browser to: `http://localhost:5000/swagger`

### 3. Test Endpoints

Use curl, Postman, or Swagger UI to test endpoints.

## Minimal API vs Controllers

| Feature | Minimal API | Controllers |
|---------|-------------|-------------|
| **Setup** | Minimal code | More boilerplate |
| **Performance** | Faster startup | Slightly slower |
| **File size** | Single file possible | Multiple files |
| **Learning curve** | Easier | More concepts |
| **Organization** | Extension methods | Controller classes |
| **Filters** | Endpoint filters | Action filters |
| **Model binding** | Parameters | Model binding |
| **Best for** | Small APIs, microservices | Large applications |

## Best Practices

### 1. Use Extension Methods for Organization

```csharp
// ✅ Good
public static void MapTodoEndpoints(this WebApplication app) { }

// ❌ Bad - everything in Program.cs
app.MapGet("/todos/1", () => { });
app.MapGet("/todos/2", () => { });
// ... 100 more endpoints
```

### 2. Use DTOs

```csharp
// ✅ Good
record CreateTodoDto(string Title, string? Description);
app.MapPost("/todos", (CreateTodoDto dto) => { });

// ❌ Bad - exposing entity directly
app.MapPost("/todos", (Todo todo) => { });
```

### 3. Return Typed Results

```csharp
// ✅ Good
return Results.Ok(todo);
return Results.NotFound();

// ❌ Bad
return todo;
return null;
```

### 4. Add OpenAPI Documentation

```csharp
// ✅ Good
app.MapGet("/todos", GetTodos)
    .WithName("GetTodos")
    .WithTags("Todos")
    .WithSummary("Get all todos")
    .Produces<List<Todo>>();
```

### 5. Use Route Groups

```csharp
// ✅ Good
var api = app.MapGroup("/api")
    .WithTags("API")
    .RequireAuthorization();

api.MapGet("/todos", GetTodos);
api.MapGet("/users", GetUsers);
```

### 6. Validate Input

```csharp
app.MapPost("/todos", (CreateTodoDto dto, TodoDb db) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { error = "Title required" });

    // Process...
});
```

## When to Use Minimal APIs

### ✅ Use Minimal APIs For:

- Microservices
- Simple CRUD APIs
- Prototypes and demos
- Learning HTTP APIs
- Cloud-native applications
- Performance-critical scenarios
- Small to medium APIs

### ❌ Use Controllers For:

- Large, complex applications
- Need for action filters
- Heavy model validation
- Team prefers MVC pattern
- Existing controller-based codebase

## Learn More

- [Minimal APIs Overview](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Minimal APIs Tutorial](https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api)
- [Route Constraints](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-constraints)
- [OpenAPI Support](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi)

## Related Projects

- **05-RazorPages**: Web UI with Razor Pages
- **06-Middlewares**: ASP.NET Core middleware
- **01-04**: Entity Framework Core examples
