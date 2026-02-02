using System.Net;
using Common.IntegrationTests;
using Portfolio.Api.Contracts.Scheduling;

namespace Scheduling.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IntegrationTestBase<SchedulingWebApplicationFactory>
{
    protected override SchedulingWebApplicationFactory CreateFactory(string connectionString)
        => new(connectionString);

    #region Profile Helpers

    protected async Task<ProfileResponse> CreateIndividualProfileAsync(string? displayName = null)
    {
        var request = new CreateProfileRequest("Individual", displayName, null);
        var response = await PostAsync(Urls.Profiles, request);
        var apiResponse = await response.ValidateSuccessAsync<ProfileResponse>(HttpStatusCode.Created);
        return apiResponse.Data!;
    }

    protected async Task<ProfileResponse> CreateBusinessProfileAsync(string businessName, string? displayName = null)
    {
        var request = new CreateProfileRequest("Business", displayName, businessName);
        var response = await PostAsync(Urls.Profiles, request);
        var apiResponse = await response.ValidateSuccessAsync<ProfileResponse>(HttpStatusCode.Created);
        return apiResponse.Data!;
    }

    protected async Task<IReadOnlyList<ProfileResponse>> GetMyProfilesAsync()
    {
        var response = await GetAsync(Urls.MyProfiles);
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<ProfileResponse>>(HttpStatusCode.OK);
        return apiResponse.Data!;
    }

    #endregion

    #region Schedule Helpers

    protected async Task<ScheduleResponse> CreateScheduleAsync(
        Guid profileId,
        string name,
        int[] daysOfWeek,
        string startTime,
        string endTime,
        int slotDurationMinutes,
        string effectiveFrom,
        string? effectiveUntil = null)
    {
        var request = new CreateScheduleRequest(
            name,
            daysOfWeek,
            startTime,
            endTime,
            slotDurationMinutes,
            effectiveFrom,
            effectiveUntil);

        var response = await PostAsync(Urls.Schedules(profileId), request);
        var apiResponse = await response.ValidateSuccessAsync<ScheduleResponse>(HttpStatusCode.Created);
        return apiResponse.Data!;
    }

    protected async Task<GenerateAvailabilitiesResponse> GenerateAvailabilitiesAsync(
        Guid profileId,
        Guid scheduleId,
        string fromDate,
        string toDate)
    {
        var request = new GenerateAvailabilitiesRequest(fromDate, toDate);
        var response = await PostAsync(Urls.GenerateAvailabilities(profileId, scheduleId), request);
        var apiResponse = await response.ValidateSuccessAsync<GenerateAvailabilitiesResponse>(HttpStatusCode.OK);
        return apiResponse.Data!;
    }

    #endregion

    #region Availability Helpers

    protected async Task<AvailabilityDetailResponse> CreateAvailabilityAsync(
        Guid profileId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int slotDurationMinutes)
    {
        var request = new CreateAvailabilityRequest(startTime, endTime, slotDurationMinutes);
        var response = await PostAsync(Urls.Availabilities(profileId), request);
        var apiResponse = await response.ValidateSuccessAsync<AvailabilityDetailResponse>(HttpStatusCode.Created);
        return apiResponse.Data!;
    }

    protected async Task<IReadOnlyList<AvailableSlotResponse>> GetAvailableSlotsAsync(
        Guid profileId,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var response = await GetAsync(Urls.AvailableSlots(profileId, Urls.FormatTime(from), Urls.FormatTime(to)));
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<AvailableSlotResponse>>(HttpStatusCode.OK);
        return apiResponse.Data!;
    }

    #endregion

    #region Appointment Helpers

    protected async Task<AppointmentResponse> BookAppointmentAsync(
        Guid hostProfileId,
        Guid guestProfileId,
        Guid timeSlotId)
    {
        var request = new BookAppointmentRequest(guestProfileId, timeSlotId);
        var response = await PostAsync(Urls.Appointments(hostProfileId), request);
        var apiResponse = await response.ValidateSuccessAsync<AppointmentResponse>(HttpStatusCode.Created);
        return apiResponse.Data!;
    }

    #endregion
}
