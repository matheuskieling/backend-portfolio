using System.Text.Json.Serialization;
using Portfolio.Api.Configuration;

namespace Portfolio.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddJwtAuthentication(configuration);
        services.AddSwaggerDocumentation();
        services.AddRateLimitingPolicies(environment);

        return services;
    }
}
