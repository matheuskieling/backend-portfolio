using System.Net;
using System.Text;
using System.Text.Json;
using Common.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Portfolio.Api.Configuration;

public static class JwtConfiguration
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT Key is not configured");
        var jwtIssuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT Issuer is not configured");
        var jwtAudience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT Audience is not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    // Skip default response
                    context.HandleResponse();

                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.ContentType = "application/json";

                    var response = new ApiResponse<object>
                    {
                        Succeeded = false,
                        Data = null,
                        StatusCode = HttpStatusCode.Unauthorized,
                        Errors = [ErrorDetail.Create("UNAUTHORIZED", "Authentication is required to access this resource.")]
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    context.Response.ContentType = "application/json";

                    var response = new ApiResponse<object>
                    {
                        Succeeded = false,
                        Data = null,
                        StatusCode = HttpStatusCode.Forbidden,
                        Errors = [ErrorDetail.Create("FORBIDDEN", "You do not have permission to access this resource.")]
                    };

                    await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
