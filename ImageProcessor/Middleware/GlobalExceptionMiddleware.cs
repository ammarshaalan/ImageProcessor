using System.Net;
using System.Text.Json;

namespace ImageProcessor.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            error = new
            {
                message = "An error occurred while processing your request",
                details = exception.Message,
                type = exception.GetType().Name
            }
        };

        switch (exception)
        {
            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case FileNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}