using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common.IntegrationTests;
using Portfolio.Api.Contracts.Identity;
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

    /// <summary>
    /// The currently authenticated test user. Set by AuthenticateAsync.
    /// Use this to access UserId, assign/remove roles, or refresh the token.
    /// </summary>
    protected TestUser CurrentTestUser { get; private set; } = null!;

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

    #region HTTP Helpers

    protected Task<HttpResponseMessage> PostAsync<T>(string url, T content)
        => Client.PostAsJsonAsync(url, content);

    protected Task<HttpResponseMessage> PostAsync(string url)
        => Client.PostAsync(url, null);

    protected Task<HttpResponseMessage> GetAsync(string url)
        => Client.GetAsync(url);

    protected Task<HttpResponseMessage> DeleteAsync(string url)
        => Client.DeleteAsync(url);

    #endregion

    #region Auth Helpers

    /// <summary>
    /// Creates a test user, authenticates them, and optionally assigns additional roles.
    /// New users automatically get the MANAGER role on registration.
    /// </summary>
    /// <param name="email">Optional email for the user (auto-generated if null)</param>
    /// <param name="isAdmin">If true, assigns the ADMIN role to the user</param>
    /// <param name="additionalRoles">Additional roles to assign</param>
    /// <returns>The TestUser instance for further operations</returns>
    protected async Task<TestUser> AuthenticateAsync(
        string? email = null,
        bool isAdmin = false,
        params string[] additionalRoles)
    {
        var user = await TestUser.CreateAsync(Client, email);
        user.Authenticate();

        if (isAdmin)
        {
            await user.AssignRoleAsync("ADMIN");
        }

        foreach (var role in additionalRoles)
        {
            await user.AssignRoleAsync(role);
        }

        // Refresh token to get updated roles/permissions if roles were assigned
        if (isAdmin || additionalRoles.Length > 0)
        {
            await user.RefreshTokenAsync();
        }

        CurrentTestUser = user;
        return user;
    }

    protected void SetAuthorizationHeader(string token)
        => Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    protected void ClearAuthorizationHeader()
        => Client.DefaultRequestHeaders.Authorization = null;

    #endregion

    #region Admin Panel Helpers

    protected async Task<GetRolesResponse> GetRolesAsync()
    {
        var response = await GetAsync("/api/admin/roles");
        return (await response.ValidateSuccessAsync<GetRolesResponse>(HttpStatusCode.OK)).Data!;
    }

    protected async Task<GetPermissionsResponse> GetPermissionsAsync()
    {
        var response = await GetAsync("/api/admin/permissions");
        return (await response.ValidateSuccessAsync<GetPermissionsResponse>(HttpStatusCode.OK)).Data!;
    }

    protected async Task<Guid> GetRoleIdByNameAsync(string roleName)
    {
        var roles = await GetRolesAsync();
        var role = roles.Roles.FirstOrDefault(r =>
            string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Role '{roleName}' not found.");
        return role.Id;
    }

    protected async Task<Guid> GetPermissionIdByNameAsync(string permissionName)
    {
        var permissions = await GetPermissionsAsync();
        var permission = permissions.Permissions.FirstOrDefault(p =>
            string.Equals(p.Name, permissionName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Permission '{permissionName}' not found.");
        return permission.Id;
    }

    #endregion
}
