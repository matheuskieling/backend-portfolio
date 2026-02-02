using System.Net;
using Common.IntegrationTests;
using Portfolio.Api.Contracts.Scheduling;
using Scheduling.IntegrationTests.Infrastructure;
using Xunit;

namespace Scheduling.IntegrationTests;

public class ScheduleEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateSchedule_WithValidData_ReturnsCreated()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        // Act
        var schedule = await CreateScheduleAsync(
            profile.Id,
            "Morning Hours",
            new[] { 1, 2, 3, 4, 5 }, // Monday-Friday
            "09:00",
            "12:00",
            60,
            "2026-02-03");

        // Assert
        Assert.NotEqual(Guid.Empty, schedule.Id);
        Assert.Equal("Morning Hours", schedule.Name);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, schedule.DaysOfWeek);
        Assert.Equal("09:00", schedule.StartTimeOfDay);
        Assert.Equal("12:00", schedule.EndTimeOfDay);
        Assert.Equal(60, schedule.SlotDurationMinutes);
        Assert.True(schedule.IsActive);
    }

    [Fact]
    public async Task CreateSchedule_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();
        await CreateScheduleAsync(profile.Id, "Morning", new[] { 1 }, "09:00", "12:00", 60, "2026-02-03");

        // Act
        var request = new CreateScheduleRequest("Morning", new[] { 2 }, "13:00", "17:00", 60, "2026-02-03", null);
        var response = await PostAsync(Urls.Schedules(profile.Id), request);

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "already exists");
    }

    [Fact]
    public async Task GetSchedules_ReturnsAllSchedulesForProfile()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();
        await CreateScheduleAsync(profile.Id, "Morning", new[] { 1, 2, 3, 4, 5 }, "09:00", "12:00", 60, "2026-02-03");
        await CreateScheduleAsync(profile.Id, "Afternoon", new[] { 1, 2, 3, 4, 5 }, "14:00", "17:00", 60, "2026-02-03");

        // Act
        var response = await GetAsync(Urls.Schedules(profile.Id));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<ScheduleResponse>>(HttpStatusCode.OK);
        Assert.Equal(2, apiResponse.Data!.Count);
    }

    [Fact]
    public async Task UpdateSchedule_WithValidData_ReturnsOk()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();
        var schedule = await CreateScheduleAsync(profile.Id, "Original", new[] { 1 }, "09:00", "12:00", 60, "2026-02-03");

        // Act
        var request = new UpdateScheduleRequest(
            "Updated Name",
            new[] { 1, 2, 3 },
            "08:00",
            "11:00",
            30,
            "2026-02-03",
            null,
            60,
            30,
            60);
        var response = await PutAsync(Urls.Schedule(profile.Id, schedule.Id), request);

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<ScheduleResponse>(HttpStatusCode.OK);
        Assert.Equal("Updated Name", apiResponse.Data!.Name);
        Assert.Equal(new[] { 1, 2, 3 }, apiResponse.Data.DaysOfWeek);
    }

    [Fact]
    public async Task PauseAndResumeSchedule_ChangesIsActiveStatus()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();
        var schedule = await CreateScheduleAsync(profile.Id, "Test", new[] { 1 }, "09:00", "12:00", 60, "2026-02-03");
        Assert.True(schedule.IsActive);

        // Act - Pause
        var pauseResponse = await PostAsync(Urls.PauseSchedule(profile.Id, schedule.Id));
        Assert.Equal(HttpStatusCode.NoContent, pauseResponse.StatusCode);

        // Verify paused
        var getResponse = await GetAsync(Urls.Schedule(profile.Id, schedule.Id));
        var pausedSchedule = (await getResponse.ValidateSuccessAsync<ScheduleDetailResponse>(HttpStatusCode.OK)).Data!;
        Assert.False(pausedSchedule.IsActive);

        // Act - Resume
        var resumeResponse = await PostAsync(Urls.ResumeSchedule(profile.Id, schedule.Id));
        Assert.Equal(HttpStatusCode.NoContent, resumeResponse.StatusCode);

        // Verify resumed
        getResponse = await GetAsync(Urls.Schedule(profile.Id, schedule.Id));
        var resumedSchedule = (await getResponse.ValidateSuccessAsync<ScheduleDetailResponse>(HttpStatusCode.OK)).Data!;
        Assert.True(resumedSchedule.IsActive);
    }

    [Fact]
    public async Task DeleteSchedule_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();
        var schedule = await CreateScheduleAsync(profile.Id, "ToDelete", new[] { 1 }, "09:00", "12:00", 60, "2026-02-03");

        // Act
        var response = await DeleteAsync(Urls.Schedule(profile.Id, schedule.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GenerateAvailabilities_CreatesAvailabilitiesForMatchingDays()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();
        // Monday only (Feb 3, 2026 is a Tuesday, Feb 2 is Monday)
        var schedule = await CreateScheduleAsync(
            profile.Id,
            "Monday Schedule",
            new[] { 1 }, // Monday
            "09:00",
            "12:00",
            60,
            "2026-02-02");

        // Act - Generate for one week (Feb 2-8, 2026)
        var result = await GenerateAvailabilitiesAsync(
            profile.Id,
            schedule.Id,
            "2026-02-02",
            "2026-02-08");

        // Assert - Should create 1 availability (Monday Feb 2)
        Assert.Equal(1, result.GeneratedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Single(result.Availabilities);
        Assert.Equal(3, result.Availabilities[0].SlotCount); // 3 hours = 3 slots of 60 min
    }

    [Fact]
    public async Task GenerateAvailabilities_SkipsOverlappingDates()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();
        var schedule = await CreateScheduleAsync(
            profile.Id,
            "Daily",
            new[] { 1, 2, 3, 4, 5 },
            "09:00",
            "12:00",
            60,
            "2026-02-02");

        // Generate first batch
        await GenerateAvailabilitiesAsync(profile.Id, schedule.Id, "2026-02-02", "2026-02-06");

        // Act - Try to generate overlapping dates
        var result = await GenerateAvailabilitiesAsync(profile.Id, schedule.Id, "2026-02-02", "2026-02-06");

        // Assert - All should be skipped
        Assert.Equal(0, result.GeneratedCount);
        Assert.Equal(5, result.SkippedCount);
    }

    [Fact]
    public async Task GenerateAvailabilities_WhenSchedulePaused_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();
        var schedule = await CreateScheduleAsync(profile.Id, "Test", new[] { 1 }, "09:00", "12:00", 60, "2026-02-02");

        await PostAsync(Urls.PauseSchedule(profile.Id, schedule.Id));

        // Act
        var request = new GenerateAvailabilitiesRequest("2026-02-02", "2026-02-08");
        var response = await PostAsync(Urls.GenerateAvailabilities(profile.Id, schedule.Id), request);

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "paused");
    }
}
