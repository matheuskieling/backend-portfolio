using Common.IntegrationTests;
using DocumentManager.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory for Identity module integration tests.
/// Inherits common configuration from PortfolioWebApplicationFactoryBase.
/// </summary>
public class PortfolioWebApplicationFactory : PortfolioWebApplicationFactoryBase
{
    public PortfolioWebApplicationFactory(string connectionString) : base(connectionString)
    {
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        // Remove existing DbContext registrations
        var descriptorsToRemove = services.Where(
            d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>) ||
                 d.ServiceType == typeof(DbContextOptions<DocumentManagerDbContext>))
            .ToList();

        foreach (var descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }

        // Re-add with test database connection
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(ConnectionString));

        services.AddDbContext<DocumentManagerDbContext>(options =>
            options.UseNpgsql(ConnectionString));
    }

    protected override void MigrateDatabase(IServiceProvider services)
    {
        // Use Migrate() instead of EnsureCreated() to run seed migrations
        var identityContext = services.GetRequiredService<IdentityDbContext>();
        identityContext.Database.Migrate();

        var documentContext = services.GetRequiredService<DocumentManagerDbContext>();
        documentContext.Database.Migrate();
    }
}
