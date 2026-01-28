using Testcontainers.PostgreSql;

namespace Common.IntegrationTests;

/// <summary>
/// Singleton PostgreSQL container shared across all integration tests.
/// Uses lazy initialization to start the container once when first accessed.
/// Each test class gets its own database within this container for full isolation,
/// enabling parallel test execution.
/// </summary>
public sealed class SharedPostgresContainer
{
    private static readonly Lazy<SharedPostgresContainer> LazyInstance = new(
        () => new SharedPostgresContainer(),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static SharedPostgresContainer Instance => LazyInstance.Value;

    private readonly PostgreSqlContainer _container;
    private readonly Task _initTask;
    private bool _isInitialized;

    private SharedPostgresContainer()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("postgres")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _initTask = InitializeContainerAsync();
    }

    private async Task InitializeContainerAsync()
    {
        await _container.StartAsync();
        _isInitialized = true;
    }

    public async Task EnsureInitializedAsync()
    {
        if (!_isInitialized)
        {
            await _initTask;
        }
    }

    public string ConnectionString => _container.GetConnectionString();
    public string Host => _container.Hostname;
    public int Port => _container.GetMappedPublicPort(5432);

    /// <summary>
    /// Creates a connection string for a specific database within the container.
    /// </summary>
    public string GetConnectionStringForDatabase(string databaseName)
    {
        return $"Host={Host};Port={Port};Database={databaseName};Username=postgres;Password=postgres";
    }
}
