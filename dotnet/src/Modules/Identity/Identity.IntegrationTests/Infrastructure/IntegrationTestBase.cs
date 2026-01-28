using System.Net.Http.Json;
using Common.IntegrationTests;
using Xunit;

namespace Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests with database-per-test isolation.
/// Each test class gets its own database, allowing parallel execution.
/// No collection attribute - tests run in parallel by default.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private string? _connectionString;
    protected PortfolioWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        // Create a unique database for this test class
        _connectionString = await TestDatabaseManager.CreateDatabaseAsync(GetType().Name);

        Factory = new PortfolioWebApplicationFactory(_connectionString);
        Client = Factory.CreateClient();
    }

    public virtual async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();

        // Drop the test database
        if (_connectionString != null)
        {
            await TestDatabaseManager.DropDatabaseAsync(_connectionString);
        }
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T content)
    {
        return await Client.PostAsJsonAsync(url, content);
    }
}
