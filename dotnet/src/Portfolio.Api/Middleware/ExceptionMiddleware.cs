using System.Net;
using System.Text.Json;
using Common.Contracts;
using Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errorCode, message) = MapException(exception);

        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new ApiResponse<object>
        {
            Succeeded = false,
            Data = null,
            StatusCode = statusCode,
            Errors = [ErrorDetail.Create(errorCode, message)]
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private (HttpStatusCode StatusCode, string Code, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            // Domain Exceptions
            DomainException domainEx when IsNotFoundException(domainEx) =>
                (HttpStatusCode.NotFound, domainEx.Code, domainEx.Message),

            DomainException domainEx when IsUnauthorizedException(domainEx) =>
                (HttpStatusCode.Forbidden, domainEx.Code, domainEx.Message),

            DomainException domainEx when IsConflictException(domainEx) =>
                (HttpStatusCode.Conflict, domainEx.Code, domainEx.Message),

            DomainException domainEx =>
                (HttpStatusCode.BadRequest, domainEx.Code, domainEx.Message),

            // Database exceptions (unique constraint violations, etc.)
            DbUpdateException dbEx when IsUniqueConstraintViolation(dbEx) =>
                (HttpStatusCode.Conflict, "DUPLICATE_ENTRY", GetUniqueConstraintMessage(dbEx)),

            // Standard exceptions
            ArgumentException argEx =>
                (HttpStatusCode.BadRequest, "INVALID_ARGUMENT", argEx.Message),

            InvalidOperationException invOpEx =>
                (HttpStatusCode.BadRequest, "INVALID_OPERATION", invOpEx.Message),

            UnauthorizedAccessException =>
                (HttpStatusCode.Forbidden, "FORBIDDEN", "Access denied."),

            // Unhandled exceptions
            _ => _environment.IsDevelopment()
                ? (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", exception.Message)
                : (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred.")
        };
    }

    private static bool IsNotFoundException(DomainException ex) =>
        ex.Code.EndsWith("_NOT_FOUND", StringComparison.OrdinalIgnoreCase);

    private static bool IsUnauthorizedException(DomainException ex) =>
        ex.Code.StartsWith("UNAUTHORIZED", StringComparison.OrdinalIgnoreCase) ||
        ex.Code.Contains("ACCESS", StringComparison.OrdinalIgnoreCase);

    private static bool IsConflictException(DomainException ex) =>
        ex.Code.Contains("ALREADY_EXISTS", StringComparison.OrdinalIgnoreCase) ||
        ex.Code.Contains("DUPLICATE", StringComparison.OrdinalIgnoreCase) ||
        ex.Code.Contains("CONFLICT", StringComparison.OrdinalIgnoreCase);

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException?.Message?.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) == true ||
        ex.InnerException?.Message?.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) == true ||
        ex.InnerException?.Message?.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) == true;

    private static string GetUniqueConstraintMessage(DbUpdateException ex) =>
        "A record with the same unique value already exists.";
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
