using System.Net.Http.Json;
using Common.IntegrationTests;
using Xunit;

namespace Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests with database-per-test isolation.
/// Each test method gets its own database, allowing full parallel execution.
/// xUnit creates a new instance per test method, so InitializeAsync runs per test.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly string _testId = Guid.NewGuid().ToString("N")[..8];
    private string? _connectionString;
    protected PortfolioWebApplicationFactory Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        // Create a unique database for this test instance (one per test method)
        _connectionString = await TestDatabaseManager.CreateDatabaseAsync($"{GetType().Name}_{_testId}");

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
