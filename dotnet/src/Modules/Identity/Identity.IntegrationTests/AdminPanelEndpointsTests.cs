using System.Net;
using Common.IntegrationTests;
using Identity.IntegrationTests.Infrastructure;
using Portfolio.Api.Contracts.Identity;
using Xunit;

namespace Identity.IntegrationTests;

public class AdminPanelEndpointsTests : IntegrationTestBase
{
    #region Permissions - GET

    [Fact]
    public async Task GetPermissions_Authenticated_ReturnsOkWithPermissions()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync("/api/admin/permissions");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<GetPermissionsResponse>(HttpStatusCode.OK);
        Assert.NotEmpty(apiResponse.Data!.Permissions);
        Assert.Contains(apiResponse.Data.Permissions, p => p.Name.StartsWith("admin:"));
    }

    [Fact]
    public async Task GetPermissions_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await GetAsync("/api/admin/permissions");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPermissionById_ExistingPermission_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync();
        var permissions = await GetPermissionsAsync();
        var firstPermission = permissions.Permissions.First();

        // Act
        var response = await GetAsync($"/api/admin/permissions/{firstPermission.Id}");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<PermissionResponse>(HttpStatusCode.OK);
        Assert.Equal(firstPermission.Id, apiResponse.Data!.Id);
        Assert.Equal(firstPermission.Name, apiResponse.Data.Name);
    }

    [Fact]
    public async Task GetPermissionById_NonExistent_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync($"/api/admin/permissions/{Guid.NewGuid()}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound);
    }

    #endregion

    #region Permissions - Create/Delete

    [Fact]
    public async Task CreatePermission_AsAdmin_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);

        // Act
        var response = await PostAsync("/api/admin/permissions", new
        {
            Name = "test:permission",
            Description = "Test permission"
        });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<CreatePermissionResponse>(HttpStatusCode.Created);
        Assert.NotEqual(Guid.Empty, apiResponse.Data!.Id);
        Assert.Equal("test:permission", apiResponse.Data.Name); // Should be lowercased
    }

    [Fact]
    public async Task CreatePermission_DuplicateName_ReturnsConflict()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);
        await PostAsync("/api/admin/permissions", new { Name = "duplicate:permission" });

        // Act
        var response = await PostAsync("/api/admin/permissions", new { Name = "duplicate:permission" });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeletePermission_AsAdmin_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);
        var createResponse = await PostAsync("/api/admin/permissions", new { Name = "todelete:permission" });
        var created = (await createResponse.ValidateSuccessAsync<CreatePermissionResponse>(HttpStatusCode.Created)).Data!;

        // Act
        var response = await DeleteAsync($"/api/admin/permissions/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's deleted
        var getResponse = await GetAsync($"/api/admin/permissions/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeletePermission_NonExistent_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);

        // Act
        var response = await DeleteAsync($"/api/admin/permissions/{Guid.NewGuid()}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound);
    }

    #endregion

    #region Roles - GET

    [Fact]
    public async Task GetRoles_Authenticated_ReturnsOkWithRoles()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync("/api/admin/roles");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<GetRolesResponse>(HttpStatusCode.OK);
        Assert.NotEmpty(apiResponse.Data!.Roles);
        Assert.Contains(apiResponse.Data.Roles, r => r.Name == "ADMIN");
        Assert.Contains(apiResponse.Data.Roles, r => r.Name == "MANAGER");
    }

    [Fact]
    public async Task GetRoles_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await GetAsync("/api/admin/roles");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetRoleById_ExistingRole_ReturnsOkWithPermissions()
    {
        // Arrange
        await AuthenticateAsync();
        var adminRoleId = await GetRoleIdByNameAsync("ADMIN");

        // Act
        var response = await GetAsync($"/api/admin/roles/{adminRoleId}");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<RoleResponse>(HttpStatusCode.OK);
        Assert.Equal("ADMIN", apiResponse.Data!.Name);
        Assert.NotEmpty(apiResponse.Data.Permissions);
    }

    [Fact]
    public async Task GetRoleById_NonExistent_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync($"/api/admin/roles/{Guid.NewGuid()}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound);
    }

    #endregion

    #region Roles - Create/Delete

    [Fact]
    public async Task CreateRole_AsAdmin_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);

        // Act
        var response = await PostAsync("/api/admin/roles", new
        {
            Name = "testrole",
            Description = "Test role"
        });

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<CreateRoleResponse>(HttpStatusCode.Created);
        Assert.NotEqual(Guid.Empty, apiResponse.Data!.Id);
        Assert.Equal("TESTROLE", apiResponse.Data.Name); // Should be uppercased
    }

    [Fact]
    public async Task CreateRole_DuplicateName_ReturnsConflict()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);
        await PostAsync("/api/admin/roles", new { Name = "duplicaterole" });

        // Act
        var response = await PostAsync("/api/admin/roles", new { Name = "duplicaterole" });

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteRole_AsAdmin_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);
        var createResponse = await PostAsync("/api/admin/roles", new { Name = "todeleterole" });
        var created = (await createResponse.ValidateSuccessAsync<CreateRoleResponse>(HttpStatusCode.Created)).Data!;

        // Act
        var response = await DeleteAsync($"/api/admin/roles/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's deleted
        var getResponse = await GetAsync($"/api/admin/roles/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    #endregion

    #region Roles - Permission Assignment

    [Fact]
    public async Task AssignPermissionToRole_AsAdmin_ReturnsRoleWithPermissions()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);

        // Create a new role
        var roleResponse = await PostAsync("/api/admin/roles", new { Name = "permissiontestrole" });
        var role = (await roleResponse.ValidateSuccessAsync<CreateRoleResponse>(HttpStatusCode.Created)).Data!;

        // Create a new permission
        var permissionResponse = await PostAsync("/api/admin/permissions", new { Name = "test:assign" });
        var permission = (await permissionResponse.ValidateSuccessAsync<CreatePermissionResponse>(HttpStatusCode.Created)).Data!;

        // Act
        var response = await PostAsync($"/api/admin/roles/{role.Id}/permissions/{permission.Id}");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<RoleResponse>(HttpStatusCode.OK);
        Assert.Equal(role.Id, apiResponse.Data!.Id);
        Assert.Equal("PERMISSIONTESTROLE", apiResponse.Data.Name);
        Assert.Contains("test:assign", apiResponse.Data.Permissions);
    }

    [Fact]
    public async Task RemovePermissionFromRole_AsAdmin_ReturnsRoleWithoutPermission()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);

        // Create a role with a permission
        var roleResponse = await PostAsync("/api/admin/roles", new { Name = "removePermRole" });
        var role = (await roleResponse.ValidateSuccessAsync<CreateRoleResponse>(HttpStatusCode.Created)).Data!;

        var permissionResponse = await PostAsync("/api/admin/permissions", new { Name = "test:remove" });
        var permission = (await permissionResponse.ValidateSuccessAsync<CreatePermissionResponse>(HttpStatusCode.Created)).Data!;

        await PostAsync($"/api/admin/roles/{role.Id}/permissions/{permission.Id}");

        // Act
        var response = await DeleteAsync($"/api/admin/roles/{role.Id}/permissions/{permission.Id}");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<RoleResponse>(HttpStatusCode.OK);
        Assert.Equal(role.Id, apiResponse.Data!.Id);
        Assert.Equal("REMOVEPERMROLE", apiResponse.Data.Name);
        Assert.DoesNotContain("test:remove", apiResponse.Data.Permissions);
    }

    #endregion

    #region Users - GET

    [Fact]
    public async Task GetUsers_Authenticated_ReturnsOnlyCurrentUser()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync("/api/admin/users");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<GetUsersResponse>(HttpStatusCode.OK);
        Assert.Single(apiResponse.Data!.Users);
        Assert.Equal(CurrentTestUser.UserId, apiResponse.Data.Users.First().Id);
    }

    [Fact]
    public async Task GetUsers_MultipleUsersExist_ReturnsOnlyCurrentUser()
    {
        // Arrange - Create multiple users
        await AuthenticateAsync("user1@test.com");
        var user1Id = CurrentTestUser.UserId;

        await AuthenticateAsync("user2@test.com");
        var user2Id = CurrentTestUser.UserId;

        await AuthenticateAsync("user3@test.com");
        var user3Id = CurrentTestUser.UserId;

        // Act - Each user should only see themselves
        var response = await GetAsync("/api/admin/users");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<GetUsersResponse>(HttpStatusCode.OK);
        Assert.Single(apiResponse.Data!.Users);
        Assert.Equal(user3Id, apiResponse.Data.Users.First().Id); // Only user3 (current user)
        Assert.DoesNotContain(apiResponse.Data.Users, u => u.Id == user1Id);
        Assert.DoesNotContain(apiResponse.Data.Users, u => u.Id == user2Id);
    }

    [Fact]
    public async Task GetUsers_AsAdmin_StillReturnsOnlyCurrentUser()
    {
        // Arrange - Create users, one becomes admin
        await AuthenticateAsync("regular@test.com");
        var regularUserId = CurrentTestUser.UserId;

        await AuthenticateAsync("admin@test.com", isAdmin: true);
        var adminUserId = CurrentTestUser.UserId;

        // Act - Even admin only sees themselves (demo restriction)
        var response = await GetAsync("/api/admin/users");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<GetUsersResponse>(HttpStatusCode.OK);
        Assert.Single(apiResponse.Data!.Users);
        Assert.Equal(adminUserId, apiResponse.Data.Users.First().Id);
        Assert.DoesNotContain(apiResponse.Data.Users, u => u.Id == regularUserId);
    }

    [Fact]
    public async Task GetUsers_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await GetAsync("/api/admin/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_OwnUser_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync($"/api/admin/users/{CurrentTestUser.UserId}");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<UserDetailResponse>(HttpStatusCode.OK);
        Assert.Equal(CurrentTestUser.UserId, apiResponse.Data!.Id);
        Assert.Contains("MANAGER", apiResponse.Data.Roles);
    }

    [Fact]
    public async Task GetUserById_OtherUser_AsManger_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsync("user1@test.com");
        var user1Id = CurrentTestUser.UserId;

        await AuthenticateAsync("user2@test.com");

        // Act
        var response = await GetAsync($"/api/admin/users/{user1Id}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserById_OtherUser_AsAdmin_ReturnsForbidden_PrivacyProtection()
    {
        // Arrange
        await AuthenticateAsync("user1@test.com");
        var user1Id = CurrentTestUser.UserId;

        await AuthenticateAsync("admin@test.com", isAdmin: true);

        // Act - Even admin cannot view other users (privacy protection)
        var response = await GetAsync($"/api/admin/users/{user1Id}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCurrentUser_Authenticated_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await GetAsync("/api/admin/me");

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<UserDetailResponse>(HttpStatusCode.OK);
        Assert.Equal(CurrentTestUser.UserId, apiResponse.Data!.Id);
        Assert.Equal(CurrentTestUser.Email, apiResponse.Data.Email);
        Assert.Contains("MANAGER", apiResponse.Data.Roles);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var response = await GetAsync("/api/admin/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region Users - Role Assignment

    [Fact]
    public async Task AssignRoleToUser_SelfAssign_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var adminRoleId = await GetRoleIdByNameAsync("ADMIN");

        // Act
        var response = await PostAsync($"/api/admin/users/{CurrentTestUser.UserId}/roles/{adminRoleId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Refresh token and verify
        await CurrentTestUser.RefreshTokenAsync();
        var meResponse = await GetAsync("/api/admin/me");
        var me = (await meResponse.ValidateSuccessAsync<UserDetailResponse>(HttpStatusCode.OK)).Data!;
        Assert.Contains("ADMIN", me.Roles);
    }

    [Fact]
    public async Task AssignRoleToUser_OtherUser_AsManager_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsync("user1@test.com");
        var user1Id = CurrentTestUser.UserId;

        await AuthenticateAsync("user2@test.com");
        var adminRoleId = await GetRoleIdByNameAsync("ADMIN");

        // Act
        var response = await PostAsync($"/api/admin/users/{user1Id}/roles/{adminRoleId}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignRoleToUser_OtherUser_AsAdmin_ReturnsForbidden_PrivacyProtection()
    {
        // Arrange
        await AuthenticateAsync("user1@test.com");
        var user1Id = CurrentTestUser.UserId;

        await AuthenticateAsync("admin@test.com", isAdmin: true);
        var adminRoleId = await GetRoleIdByNameAsync("ADMIN");

        // Act - Even admin cannot modify other users' roles (privacy protection)
        var response = await PostAsync($"/api/admin/users/{user1Id}/roles/{adminRoleId}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveRoleFromUser_SelfRemove_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();

        // First, self-assign ADMIN
        var adminRoleId = await GetRoleIdByNameAsync("ADMIN");
        await PostAsync($"/api/admin/users/{CurrentTestUser.UserId}/roles/{adminRoleId}");
        await CurrentTestUser.RefreshTokenAsync();

        // Act - Remove ADMIN role
        var response = await DeleteAsync($"/api/admin/users/{CurrentTestUser.UserId}/roles/{adminRoleId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify role is removed
        await CurrentTestUser.RefreshTokenAsync();
        var meResponse = await GetAsync("/api/admin/me");
        var me = (await meResponse.ValidateSuccessAsync<UserDetailResponse>(HttpStatusCode.OK)).Data!;
        Assert.DoesNotContain("ADMIN", me.Roles);
    }

    [Fact]
    public async Task RemoveRoleFromUser_OtherUser_AsManager_ReturnsForbidden()
    {
        // Arrange
        await AuthenticateAsync("user1@test.com");
        var user1Id = CurrentTestUser.UserId;
        var managerRoleId = await GetRoleIdByNameAsync("MANAGER");

        await AuthenticateAsync("user2@test.com");

        // Act
        var response = await DeleteAsync($"/api/admin/users/{user1Id}/roles/{managerRoleId}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RemoveRoleFromUser_OtherUser_AsAdmin_ReturnsForbidden_PrivacyProtection()
    {
        // Arrange
        await AuthenticateAsync("user1@test.com");
        var user1Id = CurrentTestUser.UserId;
        var managerRoleId = await GetRoleIdByNameAsync("MANAGER");

        await AuthenticateAsync("admin@test.com", isAdmin: true);

        // Act - Even admin cannot modify other users' roles (privacy protection)
        var response = await DeleteAsync($"/api/admin/users/{user1Id}/roles/{managerRoleId}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AssignRoleToUser_NonExistentRole_ReturnsNotFound()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await PostAsync($"/api/admin/users/{CurrentTestUser.UserId}/roles/{Guid.NewGuid()}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignRoleToUser_NonExistentUser_ReturnsForbidden_PrivacyProtection()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);
        var adminRoleId = await GetRoleIdByNameAsync("ADMIN");

        // Act - Even admin cannot modify other users (privacy protection)
        // Returns Forbidden before checking if user exists
        var response = await PostAsync($"/api/admin/users/{Guid.NewGuid()}/roles/{adminRoleId}");

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Complex Scenarios

    [Fact]
    public async Task FullAdminWorkflow_CreateRoleWithPermissions_AssignToUser()
    {
        // Arrange
        await AuthenticateAsync(isAdmin: true);

        // Create a new permission
        var permResponse = await PostAsync("/api/admin/permissions", new
        {
            Name = "reports:generate",
            Description = "Can generate reports"
        });
        var permission = (await permResponse.ValidateSuccessAsync<CreatePermissionResponse>(HttpStatusCode.Created)).Data!;

        // Create a new role
        var roleResponse = await PostAsync("/api/admin/roles", new
        {
            Name = "reporter",
            Description = "Can generate reports"
        });
        var role = (await roleResponse.ValidateSuccessAsync<CreateRoleResponse>(HttpStatusCode.Created)).Data!;

        // Assign permission to role
        await PostAsync($"/api/admin/roles/{role.Id}/permissions/{permission.Id}");

        // Assign role to current user
        await PostAsync($"/api/admin/users/{CurrentTestUser.UserId}/roles/{role.Id}");

        // Refresh token
        await CurrentTestUser.RefreshTokenAsync();

        // Act - Verify user has the role and permission
        var meResponse = await GetAsync("/api/admin/me");
        var me = (await meResponse.ValidateSuccessAsync<UserDetailResponse>(HttpStatusCode.OK)).Data!;

        // Assert
        Assert.Contains("REPORTER", me.Roles);
        Assert.Contains("reports:generate", me.Permissions);
    }

    [Fact]
    public async Task PrivacyProtection_AdminCannotAccessOtherUsers()
    {
        // Arrange - Create two users
        await AuthenticateAsync("regular@test.com");
        var regularUserId = CurrentTestUser.UserId;

        await AuthenticateAsync("admin@test.com", isAdmin: true);
        var adminRoleId = await GetRoleIdByNameAsync("ADMIN");

        // Act & Assert - Admin CANNOT view other user (privacy protection)
        var viewResponse = await GetAsync($"/api/admin/users/{regularUserId}");
        await viewResponse.ValidateFailureAsync(HttpStatusCode.Forbidden);

        // Admin CANNOT assign roles to other user (privacy protection)
        var assignResponse = await PostAsync($"/api/admin/users/{regularUserId}/roles/{adminRoleId}");
        await assignResponse.ValidateFailureAsync(HttpStatusCode.Forbidden);

        // Admin CANNOT remove roles from other user (privacy protection)
        var managerRoleId = await GetRoleIdByNameAsync("MANAGER");
        var removeResponse = await DeleteAsync($"/api/admin/users/{regularUserId}/roles/{managerRoleId}");
        await removeResponse.ValidateFailureAsync(HttpStatusCode.Forbidden);
    }

    #endregion
}
