using System.Net;
using Common.IntegrationTests;
using Identity.IntegrationTests.Infrastructure;
using Xunit;

namespace Identity.IntegrationTests;

public class AuthEndpointsTests : IntegrationTestBase
{

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
        var apiResponse = await response.ValidateSuccessAsync<RegisterResponse>(HttpStatusCode.Created);
        Assert.NotEqual(Guid.Empty, apiResponse.Data!.UserId);
        Assert.Equal("test@example.com", apiResponse.Data.Email);
        Assert.Equal("John Doe", apiResponse.Data.FullName);
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
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "already exists");
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
        var apiResponse = await response.ValidateSuccessAsync<LoginResponse>(HttpStatusCode.OK);
        Assert.NotEmpty(apiResponse.Data!.Token);
        Assert.NotEqual(Guid.Empty, apiResponse.Data.UserId);
        Assert.Equal("login@example.com", apiResponse.Data.Email);
        Assert.Equal("Jane Doe", apiResponse.Data.FullName);
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
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "invalid");
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
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "not found");
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
        var apiResponse = await response.ValidateSuccessAsync<LoginResponse>(HttpStatusCode.OK);

        // Decode JWT and verify claims
        var tokenParts = apiResponse.Data!.Token.Split('.');
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
    private record LoginResponse(string Token, Guid UserId, string Email, string FullName, IReadOnlyCollection<string> Roles);
}