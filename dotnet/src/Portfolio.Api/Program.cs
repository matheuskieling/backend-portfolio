using DotNetEnv;
using Identity.Infrastructure;
using Portfolio.Api;
using Portfolio.Api.Configuration;

// Load .env file from solution root
var solutionRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var envPath = Path.Combine(solutionRoot, ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Modules
builder.Services.AddIdentityModule(builder.Configuration);

// API services (authentication, controllers, etc.)
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.ApplyMigrations();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
