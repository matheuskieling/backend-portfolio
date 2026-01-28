using Npgsql;

namespace Common.IntegrationTests;

/// <summary>
/// Manages database creation and cleanup for integration tests.
/// </summary>
public static class TestDatabaseManager
{
    /// <summary>
    /// Creates a new database with a unique name based on the test class.
    /// Uses the shared singleton container.
    /// </summary>
    public static async Task<string> CreateDatabaseAsync(string testClassName)
    {
        var container = SharedPostgresContainer.Instance;
        await container.EnsureInitializedAsync();

        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var maxClassNameLength = 63 - 6 - uniqueSuffix.Length; // "test_" + "_" + suffix
        var truncatedClassName = testClassName.Length > maxClassNameLength
            ? testClassName[..maxClassNameLength]
            : testClassName;
        var databaseName = $"test_{truncatedClassName}_{uniqueSuffix}".ToLowerInvariant();

        await using var connection = new NpgsqlConnection(container.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
        await command.ExecuteNonQueryAsync();

        return container.GetConnectionStringForDatabase(databaseName);
    }

    /// <summary>
    /// Drops a database by connection string.
    /// </summary>
    public static async Task DropDatabaseAsync(string connectionString)
    {
        var container = SharedPostgresContainer.Instance;
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        if (string.IsNullOrEmpty(databaseName) || databaseName == "postgres")
            return;

        await using var connection = new NpgsqlConnection(container.ConnectionString);
        await connection.OpenAsync();

        // Terminate existing connections
        await using var terminateCommand = connection.CreateCommand();
        terminateCommand.CommandText = $@"
            SELECT pg_terminate_backend(pg_stat_activity.pid)
            FROM pg_stat_activity
            WHERE pg_stat_activity.datname = '{databaseName}'
            AND pid <> pg_backend_pid()";
        await terminateCommand.ExecuteNonQueryAsync();

        // Drop the database
        await using var dropCommand = connection.CreateCommand();
        dropCommand.CommandText = $"DROP DATABASE IF EXISTS \"{databaseName}\"";
        await dropCommand.ExecuteNonQueryAsync();
    }
}
