using System.Net;
using Common.IntegrationTests;
using Portfolio.Api.Contracts.Scheduling;
using Scheduling.IntegrationTests.Infrastructure;
using Xunit;

namespace Scheduling.IntegrationTests;

public class AvailabilityEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateAvailability_WithValidData_ReturnsCreatedWithSlots()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var endTime = startTime.AddHours(3);

        // Act
        var availability = await CreateAvailabilityAsync(profile.Id, startTime, endTime, 60);

        // Assert
        Assert.NotEqual(Guid.Empty, availability.Id);
        Assert.Equal(60, availability.SlotDurationMinutes);
        Assert.Equal(3, availability.TimeSlots.Count); // 3 hours = 3 slots
        Assert.All(availability.TimeSlots, s => Assert.Equal("Available", s.Status));
    }

    [Fact]
    public async Task CreateAvailability_WithOverlappingTime_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var endTime = startTime.AddHours(3);

        await CreateAvailabilityAsync(profile.Id, startTime, endTime, 60);

        // Act - Create overlapping availability
        var request = new CreateAvailabilityRequest(startTime.AddHours(1), endTime.AddHours(1), 60);
        var response = await PostAsync(Urls.Availabilities(profile.Id), request);

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "overlaps");
    }

    [Fact]
    public async Task GetAvailabilities_ReturnsAllForProfile()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        var baseTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        await CreateAvailabilityAsync(profile.Id, baseTime, baseTime.AddHours(2), 60);
        await CreateAvailabilityAsync(profile.Id, baseTime.AddDays(1), baseTime.AddDays(1).AddHours(2), 60);

        // Act
        var response = await GetAsync(Urls.Availabilities(profile.Id));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<AvailabilityResponse>>(HttpStatusCode.OK);
        Assert.Equal(2, apiResponse.Data!.Count);
    }

    [Fact]
    public async Task GetAvailabilities_WithDateFilter_ReturnsFilteredResults()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        var baseTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        await CreateAvailabilityAsync(profile.Id, baseTime, baseTime.AddHours(2), 60);
        await CreateAvailabilityAsync(profile.Id, baseTime.AddDays(5), baseTime.AddDays(5).AddHours(2), 60);

        // Act - Filter to only include first availability
        var from = Urls.FormatTime(baseTime.AddHours(-1));
        var to = Urls.FormatTime(baseTime.AddHours(3));
        var response = await GetAsync(Urls.Availabilities(profile.Id, from, to));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<AvailabilityResponse>>(HttpStatusCode.OK);
        Assert.Single(apiResponse.Data!);
    }

    [Fact]
    public async Task GetAvailabilityById_ReturnsDetailWithSlots()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var created = await CreateAvailabilityAsync(profile.Id, startTime, startTime.AddHours(2), 30);

        // Act
        var response = await GetAsync(Urls.Availability(profile.Id, created.Id));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<AvailabilityDetailResponse>(HttpStatusCode.OK);
        Assert.Equal(created.Id, apiResponse.Data!.Id);
        Assert.Equal(4, apiResponse.Data.TimeSlots.Count); // 2 hours with 30-min slots = 4 slots
    }

    [Fact]
    public async Task DeleteAvailability_WithNoBookings_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var availability = await CreateAvailabilityAsync(profile.Id, startTime, startTime.AddHours(2), 60);

        // Act
        var response = await DeleteAsync(Urls.Availability(profile.Id, availability.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailableSlots_ReturnsOnlyAvailableSlots()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        await CreateAvailabilityAsync(profile.Id, startTime, startTime.AddHours(2), 60);

        // Clear auth - this endpoint is public
        ClearAuthorizationHeader();

        // Act
        var slots = await GetAvailableSlotsAsync(profile.Id, startTime.AddHours(-1), startTime.AddHours(3));

        // Assert
        Assert.Equal(2, slots.Count);
        Assert.All(slots, s =>
        {
            Assert.NotEqual(Guid.Empty, s.Id);
            Assert.NotEqual(Guid.Empty, s.AvailabilityId);
        });
    }
}
