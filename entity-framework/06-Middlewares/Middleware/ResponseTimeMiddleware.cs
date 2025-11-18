using System.Diagnostics;

namespace MiddlewaresApp.Middleware;

/// <summary>
/// Middleware that measures and adds response time to headers
/// </summary>
public class ResponseTimeMiddleware
{
    private readonly RequestDelegate _next;

    public ResponseTimeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var watch = Stopwatch.StartNew();

        // Process request
        await _next(context);

        watch.Stop();
        var responseTimeMs = watch.ElapsedMilliseconds;

        // Add response time header
        context.Response.Headers.Add("X-Response-Time-ms", responseTimeMs.ToString());
    }
}

public static class ResponseTimeMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseTime(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ResponseTimeMiddleware>();
    }
}
