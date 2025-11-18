using Microsoft.EntityFrameworkCore;
using MinimalAPIs.Data;
using MinimalAPIs.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// ============================================
// Configure Services
// ============================================

// Add DbContext
builder.Services.AddDbContext<TodoDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=todos.db"));

// Add API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Todo API",
        Description = "A minimal API for managing todos built with ASP.NET Core 7",
        Version = "v1",
        Contact = new()
        {
            Name = "API Support",
            Email = "support@todoapi.com"
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ============================================
// Ensure Database is Created
// ============================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDb>();
    db.Database.EnsureCreated();
}

// ============================================
// Configure Middleware Pipeline
// ============================================

app.UseCors();

// Enable Swagger in all environments for demo purposes
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root
});

// ============================================
// Map Endpoints
// ============================================

// Root endpoint
app.MapGet("/health", () => new
{
    status = "Healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
})
.WithName("HealthCheck")
.WithTags("Health")
.WithSummary("Health check endpoint");

// Map all Todo endpoints
app.MapTodoEndpoints();

// ============================================
// Additional Minimal API Examples
// ============================================

// Simple GET endpoint
app.MapGet("/api/examples/hello", () => "Hello from Minimal API!")
    .WithTags("Examples")
    .WithSummary("Simple hello endpoint");

// GET with route parameter
app.MapGet("/api/examples/greet/{name}", (string name) =>
    new { message = $"Hello, {name}!", timestamp = DateTime.UtcNow })
    .WithTags("Examples")
    .WithSummary("Greet with name");

// GET with query parameters
app.MapGet("/api/examples/calculate", (int a, int b, string operation = "add") =>
{
    var result = operation.ToLower() switch
    {
        "add" => a + b,
        "subtract" => a - b,
        "multiply" => a * b,
        "divide" when b != 0 => a / b,
        _ => 0
    };

    return new
    {
        operand1 = a,
        operand2 = b,
        operation,
        result
    };
})
.WithTags("Examples")
.WithSummary("Calculate two numbers");

// POST with body
app.MapPost("/api/examples/echo", (EchoRequest request) =>
    Results.Ok(new
    {
        receivedAt = DateTime.UtcNow,
        message = request.Message,
        echo = request.Message
    }))
    .WithTags("Examples")
    .WithSummary("Echo back the request");

// Endpoint with dependency injection
app.MapGet("/api/examples/time", (ILogger<Program> logger) =>
{
    var currentTime = DateTime.UtcNow;
    logger.LogInformation("Time requested at {Time}", currentTime);

    return new
    {
        utc = currentTime,
        local = DateTime.Now,
        timezone = TimeZoneInfo.Local.DisplayName
    };
})
.WithTags("Examples")
.WithSummary("Get current time");

// Endpoint with multiple HTTP methods
app.MapMethods("/api/examples/multi", new[] { "GET", "POST" }, (HttpRequest request) =>
{
    return new
    {
        method = request.Method,
        message = $"This endpoint handles {request.Method} requests"
    };
})
.WithTags("Examples")
.WithSummary("Multi-method endpoint");

// Endpoint returning different status codes
app.MapGet("/api/examples/status/{code:int}", (int code) =>
{
    return code switch
    {
        200 => Results.Ok(new { status = code, message = "OK" }),
        201 => Results.Created("/resource/1", new { status = code, message = "Created" }),
        204 => Results.NoContent(),
        400 => Results.BadRequest(new { status = code, message = "Bad Request" }),
        404 => Results.NotFound(new { status = code, message = "Not Found" }),
        500 => Results.Problem("Internal Server Error", statusCode: 500),
        _ => Results.StatusCode(code)
    };
})
.WithTags("Examples")
.WithSummary("Return specific status code");

// Grouped endpoints
var exampleGroup = app.MapGroup("/api/group")
    .WithTags("Grouped Examples")
    .WithOpenApi();

exampleGroup.MapGet("/item1", () => new { item = 1, name = "Item One" });
exampleGroup.MapGet("/item2", () => new { item = 2, name = "Item Two" });
exampleGroup.MapGet("/item3", () => new { item = 3, name = "Item Three" });

// ============================================
// Run Application
// ============================================

Console.WriteLine("=== Minimal API Application Started ===");
Console.WriteLine($"Swagger UI: {app.Urls.FirstOrDefault() ?? "http://localhost:5000"}");
Console.WriteLine($"API Docs: {app.Urls.FirstOrDefault() ?? "http://localhost:5000"}/swagger");
Console.WriteLine("\n📋 Available endpoints:");
Console.WriteLine("  • GET    /health                      - Health check");
Console.WriteLine("  • GET    /api/todos                   - Get all todos");
Console.WriteLine("  • GET    /api/todos/{id}              - Get todo by ID");
Console.WriteLine("  • POST   /api/todos                   - Create todo");
Console.WriteLine("  • PUT    /api/todos/{id}              - Update todo");
Console.WriteLine("  • DELETE /api/todos/{id}              - Delete todo");
Console.WriteLine("  • GET    /api/todos/completed         - Get completed todos");
Console.WriteLine("  • GET    /api/todos/pending           - Get pending todos");
Console.WriteLine("  • POST   /api/todos/{id}/complete     - Mark as complete");
Console.WriteLine("  • GET    /api/todos/stats             - Get statistics");
Console.WriteLine("\n🎯 Example endpoints at /api/examples/*");
Console.WriteLine("📖 Full API documentation at /swagger\n");

app.Run();

// ============================================
// DTOs for Examples
// ============================================

record EchoRequest(string Message);
