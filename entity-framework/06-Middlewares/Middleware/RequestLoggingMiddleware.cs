namespace MiddlewaresApp.Middleware;

/// <summary>
/// Custom middleware to log HTTP request details
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Before: Log incoming request
        var startTime = DateTime.UtcNow;
        _logger.LogInformation(
            "Incoming Request: {Method} {Path} at {Time}",
            context.Request.Method,
            context.Request.Path,
            startTime);

        // Call the next middleware in the pipeline
        await _next(context);

        // After: Log response
        var endTime = DateTime.UtcNow;
        var duration = (endTime - startTime).TotalMilliseconds;
        _logger.LogInformation(
            "Completed Request: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            duration);
    }
}

/// <summary>
/// Extension method for easy middleware registration
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
