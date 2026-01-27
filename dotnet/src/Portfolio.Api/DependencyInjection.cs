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
        services.AddControllers();

        services.AddJwtAuthentication(configuration);
        services.AddSwaggerDocumentation();
        services.AddRateLimitingPolicies(environment);

        return services;
    }
}
