using System.Net;
using Common.IntegrationTests;
using Portfolio.Api.Contracts.Identity;

namespace Identity.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for Identity module integration tests.
/// Inherits common functionality from IntegrationTestBase&lt;TFactory&gt;.
/// </summary>
public abstract class IntegrationTestBase : IntegrationTestBase<PortfolioWebApplicationFactory>
{
    protected override PortfolioWebApplicationFactory CreateFactory(string connectionString)
        => new(connectionString);

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
