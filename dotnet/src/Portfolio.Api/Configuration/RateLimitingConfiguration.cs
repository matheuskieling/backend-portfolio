using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Portfolio.Api.Configuration;

public static class RateLimitingConfiguration
{
    public const string GlobalPolicy = "global";
    public const string AuthPolicy = "auth";

    public static IServiceCollection AddRateLimitingPolicies(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.AddRateLimiter(options =>
        {
            // Disable rate limiting in test environment
            if (environment.EnvironmentName == "Testing")
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
                    RateLimitPartition.GetNoLimiter("test"));
                options.AddPolicy(GlobalPolicy, _ => RateLimitPartition.GetNoLimiter("test"));
                options.AddPolicy(AuthPolicy, _ => RateLimitPartition.GetNoLimiter("test"));
                return;
            }

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global policy: 100 requests per minute per IP
            options.AddPolicy(GlobalPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Auth policy: 5 requests per 5 minutes per IP (brute force protection)
            options.AddPolicy(AuthPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(5),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Global limiter as fallback
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    Success = false,
                    Error = new
                    {
                        Code = "RATE_LIMIT_EXCEEDED",
                        Message = "Too many requests. Please try again later."
                    }
                }, cancellationToken);
            };
        });

        return services;
    }

    private static string GetClientIp(HttpContext httpContext)
    {
        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }
}
