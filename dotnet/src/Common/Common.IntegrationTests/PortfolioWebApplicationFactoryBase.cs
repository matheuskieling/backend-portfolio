using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.IntegrationTests;

/// <summary>
/// Base WebApplicationFactory for integration tests.
/// Provides common configuration (environment, JWT key) while allowing
/// modules to define their specific DbContext setup and migrations.
/// </summary>
public abstract class PortfolioWebApplicationFactoryBase : WebApplicationFactory<Program>
{
    protected readonly string ConnectionString;

    protected PortfolioWebApplicationFactoryBase(string connectionString)
    {
        ConnectionString = connectionString;

        // Set environment variables before host is built
        Environment.SetEnvironmentVariable("DATABASE_URL", connectionString);
        Environment.SetEnvironmentVariable("JWT_KEY", "test-secret-key-for-integration-tests-minimum-32-chars");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            ConfigureTestServices(services);
        });
    }

    /// <summary>
    /// Override to configure module-specific DbContext registrations.
    /// Remove existing registrations and re-add with test database connection.
    /// </summary>
    protected abstract void ConfigureTestServices(IServiceCollection services);

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        MigrateDatabase(scope.ServiceProvider);

        return host;
    }

    /// <summary>
    /// Override to run module-specific database migrations.
    /// </summary>
    protected abstract void MigrateDatabase(IServiceProvider services);
}
