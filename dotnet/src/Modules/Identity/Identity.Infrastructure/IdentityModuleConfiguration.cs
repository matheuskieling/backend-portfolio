using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Application.UseCases.Login;
using Identity.Application.UseCases.RegisterUser;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence.Repositories;
using Identity.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure;

public static class IdentityModuleConfiguration
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is not configured");

        var connectionString = ConvertToNpgsqlConnectionString(databaseUrl);

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "dotnet_identity")));

        // Unit of Work
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<IdentityDbContext>());

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Use case handlers
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginHandler>();

        return services;
    }

    private static string ConvertToNpgsqlConnectionString(string databaseUrl)
    {
        // Already in Npgsql format (Host=...)
        if (databaseUrl.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            return databaseUrl;

        // Convert postgres:// URL to Npgsql format
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var sslMode = query["sslmode"] ?? "Require";

        return $"Host={host};Port={port};Database={database};Username={userInfo[0]};Password={userInfo[1]};SSL Mode={sslMode};Trust Server Certificate=true";
    }
}
