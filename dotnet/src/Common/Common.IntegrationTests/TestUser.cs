using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common.Contracts;

namespace Common.IntegrationTests;

/// <summary>
/// Helper class for managing test user authentication in integration tests.
/// Provides methods to authenticate, manage roles, and clear authentication.
/// </summary>
public class TestUser
{
    private readonly HttpClient _client;
    private string? _token;

    public Guid UserId { get; private set; }
    public string Email { get; private set; } = null!;

    private TestUser(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Gets the JWT token for the user.
    /// </summary>
    public string GetToken() => _token ?? throw new InvalidOperationException("User is not logged in.");

    /// <summary>
    /// Sets the Authorization header with the user's JWT token.
    /// </summary>
    public void Authenticate()
    {
        if (_token is null)
            throw new InvalidOperationException("User is not logged in.");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    /// <summary>
    /// Clears the Authorization header.
    /// </summary>
    public void Logout()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Assigns a role to the user via API (self-management).
    /// The user must be authenticated before calling this method.
    /// </summary>
    public async Task AssignRoleAsync(string roleName)
    {
        // First get the role ID
        var rolesResponse = await _client.GetAsync("/api/admin/roles");
        rolesResponse.EnsureSuccessStatusCode();

        var rolesResult = await rolesResponse.Content.ReadFromJsonAsync<ApiResponse<GetRolesResponseDto>>();
        var role = rolesResult?.Data?.Roles.FirstOrDefault(r =>
            string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Role '{roleName}' not found.");

        // Assign the role
        var response = await _client.PostAsync($"/api/admin/users/{UserId}/roles/{role.Id}", null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Removes a role from the user via API (self-management).
    /// The user must be authenticated before calling this method.
    /// </summary>
    public async Task RemoveRoleAsync(string roleName)
    {
        // First get the role ID
        var rolesResponse = await _client.GetAsync("/api/admin/roles");
        rolesResponse.EnsureSuccessStatusCode();

        var rolesResult = await rolesResponse.Content.ReadFromJsonAsync<ApiResponse<GetRolesResponseDto>>();
        var role = rolesResult?.Data?.Roles.FirstOrDefault(r =>
            string.Equals(r.Name, roleName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Role '{roleName}' not found.");

        // Remove the role
        var response = await _client.DeleteAsync($"/api/admin/users/{UserId}/roles/{role.Id}");
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Re-authenticates the user to get a fresh token with updated roles/permissions.
    /// </summary>
    public async Task RefreshTokenAsync(string password = "SecurePassword123!")
    {
        var loginRequest = new { Email, Password = password };
        var response = await _client.PostAsJsonAsync("/api/identity/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var loginResult = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
        _token = loginResult?.Data?.Token ?? throw new InvalidOperationException("Failed to refresh token.");

        // Update the authorization header
        Authenticate();
    }

    /// <summary>
    /// Creates a new test user, registers them, and logs them in.
    /// New users automatically get the MANAGER role on registration.
    /// </summary>
    public static async Task<TestUser> CreateAsync(HttpClient client, string? email = null)
    {
        var testUser = new TestUser(client);
        email ??= $"test_{Guid.NewGuid():N}@example.com";
        testUser.Email = email;

        // Register the user
        var registerRequest = new
        {
            Email = email,
            Password = "SecurePassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/identity/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<RegisterResponseDto>>();
        testUser.UserId = registerResult?.Data?.UserId ?? throw new InvalidOperationException("Failed to get user ID.");

        // Login to get token
        var loginRequest = new { Email = email, Password = "SecurePassword123!" };
        var loginResponse = await client.PostAsJsonAsync("/api/identity/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
        testUser._token = loginResult?.Data?.Token ?? throw new InvalidOperationException("Failed to get token.");

        return testUser;
    }

    #region Internal DTOs

    private record RegisterResponseDto(Guid UserId, string Email, string FullName);
    private record LoginResponseDto(string Token, Guid UserId, string Email, string FullName, IReadOnlyCollection<string> Roles);
    private record GetRolesResponseDto(IReadOnlyList<RoleDto> Roles);
    private record RoleDto(Guid Id, string Name, string? Description, DateTime CreatedAt, IReadOnlyList<string> Permissions);

    #endregion
}
