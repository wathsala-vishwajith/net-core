namespace MiddlewaresApp.Middleware;

/// <summary>
/// Middleware that adds custom headers to responses
/// </summary>
public class CustomHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public CustomHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add custom headers before response is sent
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Add("X-Custom-Header", "Middleware-Demo");
            context.Response.Headers.Add("X-Powered-By", "ASP.NET Core 7.0");
            context.Response.Headers.Add("X-Request-Id", Guid.NewGuid().ToString());
            return Task.CompletedTask;
        });

        await _next(context);
    }
}

public static class CustomHeaderMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomHeaderMiddleware>();
    }
}
