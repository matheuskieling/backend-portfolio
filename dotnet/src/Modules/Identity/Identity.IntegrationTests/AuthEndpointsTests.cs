using System.Net;
using Xunit;
using System.Net.Http.Json;
using Identity.IntegrationTests.Infrastructure;

namespace Identity.IntegrationTests;

public class AuthEndpointsTests : IntegrationTestBase
{
    public AuthEndpointsTests(PortfolioWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsCreatedWithUserInfo()
    {
        // Arrange
        var request = new
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await PostAsync("/api/identity/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        Assert.NotNull(content);
        Assert.NotEqual(Guid.Empty, content.UserId);
        Assert.Equal("test@example.com", content.Email);
        Assert.Equal("John Doe", content.FullName);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "duplicate@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        await PostAsync("/api/identity/register", request);

        // Act
        var response = await PostAsync("/api/identity/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(content);
        Assert.Contains("already exists", content.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "invalid-email",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await PostAsync("/api/identity/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact(Skip = "Password strength validation not implemented yet")]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "test@example.com",
            Password = "weak",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await PostAsync("/api/identity/register", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "login@example.com",
            Password = "SecurePassword123!",
            FirstName = "Jane",
            LastName = "Doe"
        };
        await PostAsync("/api/identity/register", registerRequest);

        var loginRequest = new
        {
            Email = "login@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await PostAsync("/api/identity/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(content);
        Assert.NotEmpty(content.Token);
        Assert.NotEqual(Guid.Empty, content.UserId);
        Assert.Equal("login@example.com", content.Email);
        Assert.Equal("Jane Doe", content.FullName);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "wrongpass@example.com",
            Password = "SecurePassword123!",
            FirstName = "Jane",
            LastName = "Doe"
        };
        await PostAsync("/api/identity/register", registerRequest);

        var loginRequest = new
        {
            Email = "wrongpass@example.com",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await PostAsync("/api/identity/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(content);
        Assert.Contains("invalid", content.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new
        {
            Email = "nonexistent@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await PostAsync("/api/identity/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(content);
        Assert.Contains("not found", content.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_TokenContainsExpectedClaims()
    {
        // Arrange
        var registerRequest = new
        {
            Email = "claims@example.com",
            Password = "SecurePassword123!",
            FirstName = "Alice",
            LastName = "Smith"
        };
        await PostAsync("/api/identity/register", registerRequest);

        var loginRequest = new
        {
            Email = "claims@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var response = await PostAsync("/api/identity/login", loginRequest);

        // Assert
        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(content);

        // Decode JWT and verify claims
        var tokenParts = content.Token.Split('.');
        Assert.Equal(3, tokenParts.Length);

        var payload = System.Text.Encoding.UTF8.GetString(
            Convert.FromBase64String(PadBase64(tokenParts[1])));

        Assert.Contains("claims@example.com", payload);
        Assert.Contains("Alice", payload);
        Assert.Contains("Smith", payload);
    }

    private static string PadBase64(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: return base64 + "==";
            case 3: return base64 + "=";
            default: return base64;
        }
    }

    private record RegisterResponse(Guid UserId, string Email, string FullName);
    private record LoginResponse(string Token, Guid UserId, string Email, string FullName, IReadOnlyList<string> Roles);
    private record ErrorResponse(string Error);
}
