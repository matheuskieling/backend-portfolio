namespace Common.Infrastructure.Persistence;

public static class ConnectionStringHelper
{
    public static string ConvertToNpgsqlConnectionString(string databaseUrl)
    {
        // Already in Npgsql format (Host=...)
        if (databaseUrl.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            return databaseUrl;

        // Convert postgres:// URL to Npgsql format
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var sslMode = query["sslmode"] ?? "Require";

        return $"Host={host};Port={port};Database={database};Username={userInfo[0]};Password={userInfo[1]};SSL Mode={sslMode};Trust Server Certificate=true";
    }
}