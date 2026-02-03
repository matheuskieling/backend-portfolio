using System.Net;
using System.Text.Json;
using Common.Contracts;
using Common.Domain;
using Common.Domain.Exceptions;
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
            // Domain exceptions - ordered by specificity
            NotFoundException ex =>
                (HttpStatusCode.NotFound, ex.Code, ex.Message),

            ForbiddenException ex =>
                (HttpStatusCode.Forbidden, ex.Code, ex.Message),

            ConflictException ex =>
                (HttpStatusCode.Conflict, ex.Code, ex.Message),

            ValidationException ex =>
                (HttpStatusCode.BadRequest, ex.Code, ex.Message),

            DomainException ex =>
                (HttpStatusCode.BadRequest, ex.Code, ex.Message),

            // Database constraint violations (safety fallback)
            DbUpdateException dbEx when ContainsUniqueConstraintViolation(dbEx) =>
                (HttpStatusCode.Conflict, "DUPLICATE_ENTRY", "A record with the same unique value already exists."),

            // Standard .NET exceptions
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

    /// <summary>
    /// Safety fallback for database unique constraint violations.
    /// Business rules should validate uniqueness before attempting to save.
    /// </summary>
    private static bool ContainsUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message;
        if (string.IsNullOrEmpty(message)) return false;

        return message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }
}
