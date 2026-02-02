using Common.Infrastructure.Persistence;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Scheduling.Infrastructure.Persistence;

public class SchedulingDbContextFactory : IDesignTimeDbContextFactory<SchedulingDbContext>
{
    public SchedulingDbContext CreateDbContext(string[] args)
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

        var optionsBuilder = new DbContextOptionsBuilder<SchedulingDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new SchedulingDbContext(optionsBuilder.Options);
    }
}
