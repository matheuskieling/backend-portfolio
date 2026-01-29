using Common.Contracts.Identity;
using Common.Infrastructure.Persistence;
using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Application.UseCases.Admin.Permissions;
using Identity.Application.UseCases.Admin.Roles;
using Identity.Application.UseCases.Admin.Users;
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

        var connectionString = ConnectionStringHelper.ConvertToNpgsqlConnectionString(databaseUrl);

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    "dotnet_identity")));

        // Unit of Work
        services.AddScoped<IIdentityUnitOfWork>(provider =>
            provider.GetRequiredService<IdentityDbContext>());

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        // Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IUserQueryService, UserQueryService>();

        // Use case handlers
        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginHandler>();

        // Admin panel handlers - Permissions
        services.AddScoped<GetPermissionsHandler>();
        services.AddScoped<GetPermissionByIdHandler>();
        services.AddScoped<CreatePermissionHandler>();
        services.AddScoped<DeletePermissionHandler>();

        // Admin panel handlers - Roles
        services.AddScoped<GetRolesHandler>();
        services.AddScoped<GetRoleByIdHandler>();
        services.AddScoped<CreateRoleHandler>();
        services.AddScoped<DeleteRoleHandler>();
        services.AddScoped<AssignPermissionToRoleHandler>();
        services.AddScoped<RemovePermissionFromRoleHandler>();

        // Admin panel handlers - Users
        services.AddScoped<GetUsersHandler>();
        services.AddScoped<GetUserByIdHandler>();
        services.AddScoped<AssignRoleToUserHandler>();
        services.AddScoped<RemoveRoleFromUserHandler>();
        services.AddScoped<GetCurrentUserHandler>();

        return services;
    }

}
