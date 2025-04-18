using System.Diagnostics;

namespace ImageProcessor.Middleware;

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
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        context.Items["CorrelationId"] = correlationId;

        var stopwatch = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation(
                "Request started: {Method} {Path} (CorrelationId: {CorrelationId})",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            await _next(context);

            stopwatch.Stop();
            _logger.LogInformation(
                "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms (CorrelationId: {CorrelationId})",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Request failed: {Method} {Path} - Duration: {Duration}ms (CorrelationId: {CorrelationId})",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                correlationId);
            throw;
        }
    }
}