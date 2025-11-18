# ASP.NET Core Middleware

This project demonstrates **ASP.NET Core Middleware** concepts, showing how to build custom middleware components and configure the request processing pipeline.

## Overview

Middleware is software that's assembled into an application pipeline to handle requests and responses. Each component:
- Chooses whether to pass the request to the next component
- Can perform work before and after the next component
- Forms a chain of delegates called one after another

## Project Structure

```
06-Middlewares/
├── Middleware/
│   ├── RequestLoggingMiddleware.cs      # Logs HTTP requests/responses
│   ├── CustomHeaderMiddleware.cs        # Adds custom headers
│   ├── ApiKeyMiddleware.cs             # API key authentication
│   ├── ExceptionHandlingMiddleware.cs  # Global exception handler
│   └── ResponseTimeMiddleware.cs       # Measures response time
├── Program.cs                          # Pipeline configuration & endpoints
├── appsettings.json                    # Configuration
├── MiddlewaresApp.csproj              # Project file
└── README.md                           # This file
```

## Middleware Pipeline

```
Request
   │
   ├──> [Exception Handling] ────────────────┐
   │                                          │
   ├──> [Response Time] ──────────────────┐   │
   │                                       │   │
   ├──> [Request Logging] ─────────────┐  │   │
   │                                    │  │   │
   ├──> [Custom Headers] ───────────┐  │  │   │
   │                                 │  │  │   │
   ├──> [API Key Auth] ──────────┐  │  │  │   │
   │                              │  │  │  │   │
   ├──> [Routing] ────────────┐  │  │  │  │   │
   │                           │  │  │  │  │   │
   └──> [Endpoint] ────────┐  │  │  │  │  │   │
                           │  │  │  │  │  │   │
Response <─────────────────┘  │  │  │  │  │   │
   └──────────────────────────┘  │  │  │  │   │
      └─────────────────────────┘  │  │  │   │
         └────────────────────────┘  │  │   │
            └───────────────────────┘  │   │
               └──────────────────────┘   │
                  └─────────────────────┘
```

## Key Concepts

### 1. Middleware Anatomy

Every middleware component follows this pattern:

```csharp
public class MyMiddleware
{
    private readonly RequestDelegate _next;

    // Constructor - receives next middleware in pipeline
    public MyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // InvokeAsync - called for each request
    public async Task InvokeAsync(HttpContext context)
    {
        // Before: Code runs before next middleware
        Console.WriteLine("Before next middleware");

        // Call next middleware in pipeline
        await _next(context);

        // After: Code runs after next middleware completes
        Console.WriteLine("After next middleware");
    }
}
```

**Location:** All files in `Middleware/` directory

### 2. Middleware Registration

Middleware is registered in `Program.cs`:

```csharp
var app = builder.Build();

// Order matters!
app.UseGlobalExceptionHandling();
app.UseResponseTime();
app.UseRequestLogging();
app.UseCustomHeaders();
app.UseApiKeyAuthentication();

app.MapGet("/", () => "Hello World");

app.Run();
```

**Location:** `Program.cs:9-26`

### 3. Extension Methods

Best practice: Create extension methods for clean registration:

```csharp
public static class MyMiddlewareExtensions
{
    public static IApplicationBuilder UseMyMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MyMiddleware>();
    }
}

// Usage
app.UseMyMiddleware();
```

**Location:** Each middleware file contains its extension method

## Custom Middleware Examples

### 1. Request Logging Middleware

Logs all incoming requests and outgoing responses with timing information.

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var startTime = DateTime.UtcNow;
    _logger.LogInformation("Incoming Request: {Method} {Path}",
        context.Request.Method,
        context.Request.Path);

    await _next(context);

    var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
    _logger.LogInformation("Completed: {Method} {Path} - {StatusCode} - {Duration}ms",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        duration);
}
```

**Features:**
- Logs request method and path
- Measures request duration
- Logs response status code
- Uses ILogger for structured logging

**Location:** `Middleware/RequestLoggingMiddleware.cs`

---

### 2. Custom Header Middleware

Adds custom headers to all HTTP responses.

```csharp
public async Task InvokeAsync(HttpContext context)
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Add("X-Custom-Header", "Middleware-Demo");
        context.Response.Headers.Add("X-Powered-By", "ASP.NET Core 7.0");
        context.Response.Headers.Add("X-Request-Id", Guid.NewGuid().ToString());
        return Task.CompletedTask;
    });

    await _next(context);
}
```

**Features:**
- Uses `OnStarting` callback to add headers
- Adds custom identification headers
- Generates unique request ID

**Location:** `Middleware/CustomHeaderMiddleware.cs`

---

### 3. API Key Middleware

Implements API key authentication for protected endpoints.

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Skip for public endpoints
    if (context.Request.Path.StartsWithSegments("/"))
    {
        await _next(context);
        return;
    }

    // Check for API key
    if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsJsonAsync(new { error = "API Key missing" });
        return;
    }

    // Validate API key
    if (!IsValidApiKey(apiKey))
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid API Key" });
        return;
    }

    await _next(context);
}
```

**Features:**
- Header-based API key authentication
- Skips authentication for public routes
- Returns appropriate HTTP status codes (401, 403)
- Short-circuits pipeline for unauthorized requests

**Location:** `Middleware/ApiKeyMiddleware.cs`

**Testing:**
```bash
# Will fail (no API key)
curl http://localhost:5000/api/hello

# Will succeed
curl -H "X-API-Key: MySecretApiKey123" http://localhost:5000/api/hello
```

---

### 4. Exception Handling Middleware

Global exception handler that catches and formats all unhandled exceptions.

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception");
        await HandleExceptionAsync(context, ex);
    }
}

private static Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    var statusCode = HttpStatusCode.InternalServerError;
    var message = "Internal server error";

    switch (exception)
    {
        case ArgumentException:
            statusCode = HttpStatusCode.BadRequest;
            break;
        case KeyNotFoundException:
            statusCode = HttpStatusCode.NotFound;
            break;
        case UnauthorizedAccessException:
            statusCode = HttpStatusCode.Unauthorized;
            break;
    }

    context.Response.StatusCode = (int)statusCode;
    return context.Response.WriteAsJsonAsync(new
    {
        error = message,
        type = exception.GetType().Name
    });
}
```

**Features:**
- Catches all unhandled exceptions
- Maps exceptions to appropriate HTTP status codes
- Returns JSON error responses
- Logs exceptions for debugging

**Location:** `Middleware/ExceptionHandlingMiddleware.cs`

---

### 5. Response Time Middleware

Measures request processing time and adds it to response headers.

```csharp
public async Task InvokeAsync(HttpContext context)
{
    var watch = Stopwatch.StartNew();

    await _next(context);

    watch.Stop();
    context.Response.Headers.Add("X-Response-Time-ms",
        watch.ElapsedMilliseconds.ToString());
}
```

**Features:**
- Uses Stopwatch for accurate timing
- Adds response time to headers
- Useful for performance monitoring

**Location:** `Middleware/ResponseTimeMiddleware.cs`

## Middleware Order

**Order is critical!** Middleware executes in the order registered:

```csharp
// ✅ Correct order
app.UseExceptionHandler();      // 1. Catch exceptions from everything below
app.UseResponseTime();          // 2. Measure total time
app.UseLogging();              // 3. Log requests
app.UseAuthentication();       // 4. Authenticate users
app.UseAuthorization();        // 5. Authorize users
app.MapControllers();          // 6. Route to endpoints

// ❌ Wrong order
app.UseAuthentication();       // Can't catch auth exceptions
app.UseExceptionHandler();     // Won't see auth errors!
```

**General Guidelines:**

1. **Exception Handling** - First (catch everything)
2. **HTTPS Redirection** - Early (security)
3. **Static Files** - Early (performance)
4. **Routing** - Middle
5. **Authentication** - Before authorization
6. **Authorization** - Before endpoints
7. **Endpoints** - Last

## Inline Middleware

Quick middleware without creating a class:

### Using Use()

```csharp
app.Use(async (context, next) =>
{
    // Before
    Console.WriteLine($"Before: {context.Request.Path}");

    await next();  // Call next middleware

    // After
    Console.WriteLine($"After: {context.Request.Path}");
});
```

### Using Run() (Terminal)

```csharp
app.Run(async context =>
{
    // Terminal middleware - doesn't call next()
    await context.Response.WriteAsync("End of pipeline");
});
```

**Location:** `Program.cs:106-112`

## Conditional Middleware

Apply middleware only to specific routes:

### Map()

```csharp
app.Map("/admin", adminApp =>
{
    adminApp.UseAdminAuthentication();
    adminApp.MapGet("/status", () => "Admin status");
});
```

### MapWhen()

```csharp
app.MapWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    apiApp =>
    {
        apiApp.UseApiKeyAuthentication();
    });
```

**Location:** `Program.cs:123-136`

## Running the Application

### 1. Run the App

```bash
dotnet run
```

### 2. Test Endpoints

**Public endpoints (no API key):**

```bash
# Home page
curl http://localhost:5000/

# Middleware info
curl http://localhost:5000/info
```

**Protected endpoints (require API key):**

```bash
# Without API key (fails with 401)
curl http://localhost:5000/api/hello

# With API key (succeeds)
curl -H "X-API-Key: MySecretApiKey123" http://localhost:5000/api/hello

# Get data
curl -H "X-API-Key: MySecretApiKey123" http://localhost:5000/api/data
```

**Exception handling:**

```bash
# Trigger an error
curl -H "X-API-Key: MySecretApiKey123" http://localhost:5000/api/error

# Not found exception
curl -H "X-API-Key: MySecretApiKey123" http://localhost:5000/api/notfound

# Bad request exception
curl -H "X-API-Key: MySecretApiKey123" http://localhost:5000/api/badrequest
```

**Check response headers:**

```bash
curl -i http://localhost:5000/info
```

You'll see custom headers:
```
X-Custom-Header: Middleware-Demo
X-Powered-By: ASP.NET Core 7.0
X-Request-Id: <guid>
X-Response-Time-ms: 5
```

## Common Use Cases

### 1. Authentication/Authorization

```csharp
public class AuthenticationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"];
        if (ValidateToken(token))
        {
            // Set user claims
            await _next(context);
        }
        else
        {
            context.Response.StatusCode = 401;
        }
    }
}
```

### 2. Request/Response Logging

```csharp
public class LoggingMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Log request
        _logger.LogInformation("Request: {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        await _next(context);

        // Log response
        _logger.LogInformation("Response: {StatusCode}",
            context.Response.StatusCode);
    }
}
```

### 3. CORS Headers

```csharp
public class CorsMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");

        if (context.Request.Method == "OPTIONS")
        {
            context.Response.StatusCode = 200;
            return;
        }

        await _next(context);
    }
}
```

### 4. Rate Limiting

```csharp
public class RateLimitingMiddleware
{
    private static Dictionary<string, int> _requests = new();

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString();

        if (_requests.ContainsKey(ip) && _requests[ip] > 100)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            return;
        }

        _requests[ip] = _requests.GetValueOrDefault(ip, 0) + 1;
        await _next(context);
    }
}
```

### 5. Request/Response Modification

```csharp
public class ResponseModificationMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var originalBody = context.Response.Body;

        using var newBody = new MemoryStream();
        context.Response.Body = newBody;

        await _next(context);

        newBody.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(newBody).ReadToEndAsync();

        // Modify response
        var modifiedResponse = responseBody.ToUpper();

        await context.Response.WriteAsync(modifiedResponse);
    }
}
```

## Best Practices

### 1. Keep Middleware Focused

Each middleware should have a single responsibility.

```csharp
// ✅ Good - single purpose
app.UseAuthentication();
app.UseLogging();
app.UseCompression();

// ❌ Bad - does too much
app.UseEverything(); // auth, logging, compression, etc.
```

### 2. Use Extension Methods

```csharp
// ✅ Good
app.UseMyMiddleware();

// ❌ Bad
app.UseMiddleware<MyMiddleware>();
```

### 3. Handle Exceptions

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error in middleware");
        throw; // Re-throw to let exception handler deal with it
    }
}
```

### 4. Don't Modify Response After Next()

```csharp
// ❌ Bad - response might already be sent
public async Task InvokeAsync(HttpContext context)
{
    await _next(context);
    context.Response.Headers.Add("X-Header", "Value"); // Too late!
}

// ✅ Good - use OnStarting
public async Task InvokeAsync(HttpContext context)
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Add("X-Header", "Value");
        return Task.CompletedTask;
    });

    await _next(context);
}
```

### 5. Use Dependency Injection

```csharp
public class MyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<MyMiddleware> _logger;
    private readonly IMyService _service;

    public MyMiddleware(
        RequestDelegate next,
        ILogger<MyMiddleware> logger,
        IMyService service) // Scoped services via InvokeAsync parameter
    {
        _next = next;
        _logger = logger;
        _service = service;
    }

    public async Task InvokeAsync(HttpContext context, IMyService service)
    {
        // Use service here
    }
}
```

## Learn More

- [ASP.NET Core Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)
- [Custom Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/write)
- [Middleware Ordering](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/#middleware-order)
- [Factory-based Middleware](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/extensibility)

## Related Projects

- **05-RazorPages**: Web UI with Razor Pages
- **07-Minimal-APIs**: Minimal API approach
