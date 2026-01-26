using System.Net.Http.Json;
using Identity.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Identity.IntegrationTests.Infrastructure;

[Collection(nameof(IntegrationTestCollection))]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PortfolioWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected IntegrationTestBase(PortfolioWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        dbContext.Users.RemoveRange(dbContext.Users);
        dbContext.Roles.RemoveRange(dbContext.Roles);
        dbContext.Permissions.RemoveRange(dbContext.Permissions);
        await dbContext.SaveChangesAsync();
    }

    protected async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T content)
    {
        return await Client.PostAsJsonAsync(url, content);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest content)
    {
        var response = await Client.PostAsJsonAsync(url, content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }
}
