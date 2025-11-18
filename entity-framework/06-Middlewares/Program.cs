using MiddlewaresApp.Middleware;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// ============================================
// MIDDLEWARE PIPELINE DEMONSTRATION
// ============================================
// Order matters! Middleware executes in the order they are added

Console.WriteLine("=== Middleware Pipeline Configuration ===\n");

// 1. Global Exception Handling (should be first to catch all exceptions)
app.UseGlobalExceptionHandling();
Console.WriteLine("✓ Exception Handling Middleware registered");

// 2. Response Time Measurement
app.UseResponseTime();
Console.WriteLine("✓ Response Time Middleware registered");

// 3. Request Logging
app.UseRequestLogging();
Console.WriteLine("✓ Request Logging Middleware registered");

// 4. Custom Headers
app.UseCustomHeaders();
Console.WriteLine("✓ Custom Headers Middleware registered");

// 5. API Key Authentication (before endpoints)
app.UseApiKeyAuthentication();
Console.WriteLine("✓ API Key Authentication Middleware registered");

Console.WriteLine("\n=== Middleware Pipeline Ready ===\n");

// ============================================
// ENDPOINTS
// ============================================

// Home endpoint (no API key required)
app.MapGet("/", () => new
{
    message = "Welcome to Middleware Demo!",
    info = "This application demonstrates various ASP.NET Core middleware concepts",
    endpoints = new[]
    {
        new { path = "/", description = "This home page (no API key required)" },
        new { path = "/info", description = "Middleware pipeline information (no API key required)" },
        new { path = "/api/hello", description = "Protected endpoint (requires API key)" },
        new { path = "/api/data", description = "Protected data endpoint (requires API key)" },
        new { path = "/api/error", description = "Endpoint that throws an exception" }
    },
    apiKey = new
    {
        header = "X-API-Key",
        value = "MySecretApiKey123",
        usage = "Add this header to access protected endpoints"
    }
});

// Info endpoint (no API key required)
app.MapGet("/info", (HttpContext context) =>
{
    var headers = context.Response.Headers
        .Where(h => h.Key.StartsWith("X-"))
        .ToDictionary(h => h.Key, h => h.Value.ToString());

    return new
    {
        message = "Middleware Pipeline Information",
        pipeline = new[]
        {
            "1. Exception Handling Middleware",
            "2. Response Time Middleware",
            "3. Request Logging Middleware",
            "4. Custom Headers Middleware",
            "5. API Key Authentication Middleware",
            "6. Routing Middleware",
            "7. Endpoint Middleware"
        },
        customHeaders = headers,
        description = new
        {
            exceptionHandling = "Catches and handles all unhandled exceptions",
            responseTime = "Measures request processing time",
            requestLogging = "Logs all incoming requests and responses",
            customHeaders = "Adds custom headers to responses",
            apiKeyAuth = "Validates API key for protected endpoints"
        }
    };
});

// Protected endpoints (require API key)
app.MapGet("/api/hello", () => new
{
    message = "Hello from protected endpoint!",
    timestamp = DateTime.UtcNow,
    authenticated = true
});

app.MapGet("/api/data", () => new
{
    data = new[]
    {
        new { id = 1, name = "Item 1", value = 100 },
        new { id = 2, name = "Item 2", value = 200 },
        new { id = 3, name = "Item 3", value = 300 }
    },
    count = 3,
    timestamp = DateTime.UtcNow
});

// Endpoint that throws an exception (to demonstrate exception middleware)
app.MapGet("/api/error", () =>
{
    throw new InvalidOperationException("This is a test exception to demonstrate error handling middleware");
});

// Endpoint that throws a not found exception
app.MapGet("/api/notfound", () =>
{
    throw new KeyNotFoundException("The requested resource was not found");
});

// Endpoint that throws an argument exception
app.MapGet("/api/badrequest", () =>
{
    throw new ArgumentException("Invalid argument provided");
});

// Inline middleware example using Use()
app.Use(async (context, next) =>
{
    // This middleware runs for every request
    Console.WriteLine($"[Inline Middleware] Processing: {context.Request.Path}");
    await next();
    Console.WriteLine($"[Inline Middleware] Completed: {context.Request.Path}");
});

// Inline middleware using Run() (terminal middleware)
app.MapGet("/terminal", () =>
{
    return new
    {
        message = "This endpoint demonstrates terminal middleware",
        description = "app.Run() creates terminal middleware that doesn't call next()"
    };
});

// Conditional middleware using MapWhen()
app.MapWhen(
    context => context.Request.Path.StartsWithSegments("/admin"),
    adminApp =>
    {
        adminApp.Use(async (context, next) =>
        {
            // Additional middleware for /admin routes only
            context.Response.Headers.Add("X-Admin-Route", "true");
            await next();
        });

        adminApp.MapGet("/admin/status", () => new
        {
            message = "Admin endpoint",
            description = "This route has additional middleware applied via MapWhen()"
        });
    });

Console.WriteLine("=== Application Started ===");
Console.WriteLine("Try these requests:\n");
Console.WriteLine("1. curl http://localhost:5000/");
Console.WriteLine("2. curl http://localhost:5000/info");
Console.WriteLine("3. curl -H \"X-API-Key: MySecretApiKey123\" http://localhost:5000/api/hello");
Console.WriteLine("4. curl -H \"X-API-Key: MySecretApiKey123\" http://localhost:5000/api/data");
Console.WriteLine("5. curl -H \"X-API-Key: MySecretApiKey123\" http://localhost:5000/api/error");
Console.WriteLine("6. curl http://localhost:5000/api/hello (without API key - will fail)");
Console.WriteLine("7. curl http://localhost:5000/admin/status\n");

app.Run();
