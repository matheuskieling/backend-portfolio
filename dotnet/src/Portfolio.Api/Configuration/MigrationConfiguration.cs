using DocumentManager.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scheduling.Infrastructure.Persistence;

namespace Portfolio.Api.Configuration;

public static class MigrationConfiguration
{
    public static void ApplyMigrations(this WebApplication app)
    {
        // Skip migrations in Testing environment (tests use EnsureCreated())
        if (app.Environment.EnvironmentName == "Testing")
        {
            return;
        }

        using var scope = app.Services.CreateScope();

        // Identity module
        var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        identityDb.Database.Migrate();

        // DocumentManager module
        var documentManagerDb = scope.ServiceProvider.GetRequiredService<DocumentManagerDbContext>();
        documentManagerDb.Database.Migrate();

        // Scheduling module
        var schedulingDb = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
        schedulingDb.Database.Migrate();
    }
}
