using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Identity.IntegrationTests.Infrastructure;

/// <summary>
/// WebApplicationFactory that uses a pre-created database connection string.
/// Each test class creates its own database and passes it to this factory.
/// </summary>
public class PortfolioWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public PortfolioWebApplicationFactory(string connectionString)
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
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<IdentityDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(_connectionString));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        dbContext.Database.EnsureCreated();

        return host;
    }
}
