using DocumentManager.Infrastructure.Persistence;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DocumentManager.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory that uses a pre-created database connection string.
/// Each test class creates its own database and passes it to this factory.
/// </summary>
public class DocumentManagerWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public DocumentManagerWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;

        // Set environment variables before host is built
        Environment.SetEnvironmentVariable("DATABASE_URL", connectionString);
        Environment.SetEnvironmentVariable("JWT_KEY", "test-secret-key-for-integration-tests-minimum-32-chars");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
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
                options.UseNpgsql(_connectionString));

            services.AddDbContext<DocumentManagerDbContext>(options =>
                options.UseNpgsql(_connectionString));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();

        // Ensure both schemas are created
        var identityContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        identityContext.Database.EnsureCreated();

        var documentContext = scope.ServiceProvider.GetRequiredService<DocumentManagerDbContext>();
        documentContext.Database.EnsureCreated();

        return host;
    }
}
