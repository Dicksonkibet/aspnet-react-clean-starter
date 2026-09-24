using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CleanStart.API.Middleware;

/// <summary>
/// Catches any unhandled exception and turns it into a consistent JSON error response
/// instead of letting it crash the request (which also avoids misleading "blocked by
/// CORS" errors in the browser console — the response dying before CORS headers
/// attach looks like a CORS failure but isn't one).
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response already started; cannot write error body.");
            return;
        }

        var (statusCode, title, detail) = MapException(context, exception);

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new
        {
            status = statusCode,
            title,
            detail = detail ?? (_env.IsDevelopment() ? exception.ToString() : null),
            traceId = context.TraceIdentifier
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }

    private static (int StatusCode, string Title, string? Detail) MapException(HttpContext context, Exception exception) => exception switch
    {
        UnauthorizedAccessException when context.User.Identity?.IsAuthenticated == true
            => (StatusCodes.Status403Forbidden, "Forbidden", "You don't have permission to do that."),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", null),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found", exception.Message),
        ValidationException validation => (StatusCodes.Status400BadRequest, "Validation failed", string.Join(" ", validation.Errors.Select(e => e.ErrorMessage))),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict", "That was changed by someone else at the same moment. Please try again."),
        InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid request", exception.Message),
        ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request", exception.Message),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", null)
    };
}

public static class GlobalExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
}
