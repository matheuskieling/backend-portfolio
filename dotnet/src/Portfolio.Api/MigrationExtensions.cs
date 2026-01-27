using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Api;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        // Identity module
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        identityDb.Database.Migrate();

        // Future modules:
        // var catalogDb = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        // catalogDb.Database.Migrate();
    }
}