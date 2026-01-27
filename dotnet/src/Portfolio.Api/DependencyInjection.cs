using Portfolio.Api.Configuration;

namespace Portfolio.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers();

        services.AddJwtAuthentication(configuration);

        return services;
    }
}
