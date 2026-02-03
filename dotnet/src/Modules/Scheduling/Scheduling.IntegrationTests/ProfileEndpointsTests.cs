using System.Net;
using Common.IntegrationTests;
using Portfolio.Api.Contracts.Scheduling;
using Scheduling.IntegrationTests.Infrastructure;
using Xunit;

namespace Scheduling.IntegrationTests;

public class ProfileEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateIndividualProfile_WithValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var profile = await CreateIndividualProfileAsync("John's Profile");

        // Assert
        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal("Individual", profile.Type);
        Assert.Equal("John's Profile", profile.DisplayName);
        Assert.Null(profile.BusinessName);
    }

    [Fact]
    public async Task CreateBusinessProfile_WithValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var profile = await CreateBusinessProfileAsync("Acme Corp", "Acme Scheduling");

        // Assert
        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal("Business", profile.Type);
        Assert.Equal("Acme Scheduling", profile.DisplayName);
        Assert.Equal("Acme Corp", profile.BusinessName);
    }

    [Fact]
    public async Task CreateIndividualProfile_WhenOneAlreadyExists_ReturnsConflict()
    {
        // Arrange
        await AuthenticateAsync();
        await CreateIndividualProfileAsync();

        // Act
        var request = new CreateProfileRequest("Individual", null, null);
        var response = await PostAsync(Urls.Profiles, request);

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Conflict, expectedErrorMessage: "already has an individual profile");
    }

    [Fact]
    public async Task CreateBusinessProfile_WithDuplicateName_ReturnsConflict()
    {
        // Arrange
        await AuthenticateAsync();
        await CreateBusinessProfileAsync("Unique Business");

        // Act
        var request = new CreateProfileRequest("Business", null, "Unique Business");
        var response = await PostAsync(Urls.Profiles, request);

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.Conflict, expectedErrorMessage: "already exists");
    }

    [Fact]
    public async Task CreateBusinessProfile_WithoutBusinessName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var request = new CreateProfileRequest("Business", null, null);
        var response = await PostAsync(Urls.Profiles, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMyProfiles_ReturnsAllUserProfiles()
    {
        // Arrange
        await AuthenticateAsync();
        await CreateIndividualProfileAsync("Individual");
        await CreateBusinessProfileAsync("Business 1");
        await CreateBusinessProfileAsync("Business 2");

        // Act
        var profiles = await GetMyProfilesAsync();

        // Assert
        Assert.Equal(3, profiles.Count);
        Assert.Single(profiles, p => p.Type == "Individual");
        Assert.Equal(2, profiles.Count(p => p.Type == "Business"));
    }

    [Fact]
    public async Task GetProfileById_WithValidId_ReturnsProfile()
    {
        // Arrange
        await AuthenticateAsync();
        var created = await CreateIndividualProfileAsync("Test Profile");

        // Clear auth to test public access
        ClearAuthorizationHeader();

        // Act
        var response = await GetAsync(Urls.Profile(created.Id));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<ProfileResponse>(HttpStatusCode.OK);
        Assert.Equal(created.Id, apiResponse.Data!.Id);
        Assert.Equal("Individual", apiResponse.Data.Type);
    }

    [Fact]
    public async Task GetProfileById_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await GetAsync(Urls.Profile(Guid.NewGuid()));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProfile_WithNoAppointments_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        // Act
        var response = await DeleteAsync(Urls.Profile(profile.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify deleted
        var profiles = await GetMyProfilesAsync();
        Assert.Empty(profiles);
    }

    [Fact]
    public async Task DeleteProfile_OwnedByOtherUser_ReturnsForbidden()
    {
        // Arrange - Create profile with first user
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        // Switch to second user
        await AuthenticateAsync();

        // Act
        var response = await DeleteAsync(Urls.Profile(profile.Id));

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProfile_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var request = new CreateProfileRequest("Individual", null, null);
        var response = await PostAsync(Urls.Profiles, request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
