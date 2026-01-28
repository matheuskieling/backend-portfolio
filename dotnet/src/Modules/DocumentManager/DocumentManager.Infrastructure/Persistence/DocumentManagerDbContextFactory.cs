using Common.Infrastructure.Persistence;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DocumentManager.Infrastructure.Persistence;

public class DocumentManagerDbContextFactory : IDesignTimeDbContextFactory<DocumentManagerDbContext>
{
    public DocumentManagerDbContext CreateDbContext(string[] args)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var envPath = Path.Combine(currentDir, ".env");

        if (File.Exists(envPath))
        {
            Env.Load(envPath);
        }

        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL")
            ?? throw new InvalidOperationException("DATABASE_URL not found in environment variables");

        var connectionString = ConnectionStringHelper.ConvertToNpgsqlConnectionString(databaseUrl);

        var optionsBuilder = new DbContextOptionsBuilder<DocumentManagerDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new DocumentManagerDbContext(optionsBuilder.Options);
    }
}
