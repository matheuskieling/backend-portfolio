using Common.IntegrationTests;
using DocumentManager.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Infrastructure.Persistence;

namespace Scheduling.IntegrationTests.Infrastructure;

public class SchedulingWebApplicationFactory : PortfolioWebApplicationFactoryBase
{
    public SchedulingWebApplicationFactory(string connectionString) : base(connectionString)
    {
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var descriptorsToRemove = services.Where(
            d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>) ||
                 d.ServiceType == typeof(DbContextOptions<DocumentManagerDbContext>) ||
                 d.ServiceType == typeof(DbContextOptions<SchedulingDbContext>))
            .ToList();

        foreach (var descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(ConnectionString));

        services.AddDbContext<DocumentManagerDbContext>(options =>
            options.UseNpgsql(ConnectionString));

        services.AddDbContext<SchedulingDbContext>(options =>
            options.UseNpgsql(ConnectionString));
    }

    protected override void MigrateDatabase(IServiceProvider services)
    {
        var identityContext = services.GetRequiredService<IdentityDbContext>();
        identityContext.Database.Migrate();

        var documentContext = services.GetRequiredService<DocumentManagerDbContext>();
        documentContext.Database.Migrate();

        var schedulingContext = services.GetRequiredService<SchedulingDbContext>();
        schedulingContext.Database.Migrate();
    }
}
