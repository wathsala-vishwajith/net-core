namespace MiddlewaresApp.Middleware;

/// <summary>
/// Middleware for API key authentication
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string API_KEY_HEADER = "X-API-Key";
    private const string VALID_API_KEY = "MySecretApiKey123"; // In production, use configuration

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip API key check for home page and info endpoints
        if (context.Request.Path.StartsWithSegments("/") ||
            context.Request.Path.StartsWithSegments("/info"))
        {
            await _next(context);
            return;
        }

        // Check for API key in header
        if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
        {
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new
            {
                error = "API Key is missing",
                message = $"Please provide a valid API key in the '{API_KEY_HEADER}' header"
            });
            return;
        }

        // Validate API key
        if (!VALID_API_KEY.Equals(extractedApiKey))
        {
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid API Key",
                message = "The provided API key is not valid"
            });
            return;
        }

        // API key is valid, continue to next middleware
        await _next(context);
    }
}

public static class ApiKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyMiddleware>();
    }
}
