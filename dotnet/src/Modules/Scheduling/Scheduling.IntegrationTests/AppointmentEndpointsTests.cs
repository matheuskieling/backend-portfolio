using System.Net;
using Common.IntegrationTests;
using Portfolio.Api.Contracts.Scheduling;
using Scheduling.IntegrationTests.Infrastructure;
using Xunit;

namespace Scheduling.IntegrationTests;

public class AppointmentEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task BookAppointment_WithValidData_ReturnsCreated()
    {
        // Arrange - Create host profile
        await AuthenticateAsync();
        var hostProfile = await CreateIndividualProfileAsync("Host");
        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var availability = await CreateAvailabilityAsync(hostProfile.Id, startTime, startTime.AddHours(2), 60);
        var timeSlotId = availability.TimeSlots.First().Id;

        // Create guest user and profile
        await AuthenticateAsync();
        var guestProfile = await CreateIndividualProfileAsync("Guest");

        // Act
        var appointment = await BookAppointmentAsync(hostProfile.Id, guestProfile.Id, timeSlotId);

        // Assert
        Assert.NotEqual(Guid.Empty, appointment.Id);
        Assert.Equal(hostProfile.Id, appointment.HostProfileId);
        Assert.Equal(guestProfile.Id, appointment.GuestProfileId);
        Assert.Equal("Scheduled", appointment.Status);
        Assert.False(appointment.IsHost);
    }

    [Fact]
    public async Task BookAppointment_SelfBooking_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var profile = await CreateIndividualProfileAsync();

        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var availability = await CreateAvailabilityAsync(profile.Id, startTime, startTime.AddHours(2), 60);
        var timeSlotId = availability.TimeSlots.First().Id;

        // Act - Try to book own slot
        var request = new BookAppointmentRequest(profile.Id, timeSlotId);
        var response = await PostAsync(Urls.Appointments(profile.Id), request);

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "cannot book");
    }

    [Fact]
    public async Task BookAppointment_AlreadyBookedSlot_ReturnsBadRequest()
    {
        // Arrange - Setup host
        await AuthenticateAsync();
        var hostProfile = await CreateIndividualProfileAsync("Host");
        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var availability = await CreateAvailabilityAsync(hostProfile.Id, startTime, startTime.AddHours(2), 60);
        var timeSlotId = availability.TimeSlots.First().Id;

        // First guest books
        await AuthenticateAsync();
        var guest1Profile = await CreateIndividualProfileAsync("Guest1");
        await BookAppointmentAsync(hostProfile.Id, guest1Profile.Id, timeSlotId);

        // Second guest tries to book same slot
        await AuthenticateAsync();
        var guest2Profile = await CreateIndividualProfileAsync("Guest2");

        // Act
        var request = new BookAppointmentRequest(guest2Profile.Id, timeSlotId);
        var response = await PostAsync(Urls.Appointments(hostProfile.Id), request);

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "not available");
    }

    [Fact]
    public async Task GetAppointments_ReturnsAppointmentsForProfile()
    {
        // Arrange
        await AuthenticateAsync();
        var hostProfile = await CreateIndividualProfileAsync("Host");
        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var availability = await CreateAvailabilityAsync(hostProfile.Id, startTime, startTime.AddHours(3), 60);

        // Guest books two appointments
        await AuthenticateAsync();
        var guestProfile = await CreateIndividualProfileAsync("Guest");
        await BookAppointmentAsync(hostProfile.Id, guestProfile.Id, availability.TimeSlots[0].Id);
        await BookAppointmentAsync(hostProfile.Id, guestProfile.Id, availability.TimeSlots[1].Id);

        // Act - Get as guest
        var response = await GetAsync(Urls.Appointments(guestProfile.Id));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<AppointmentResponse>>(HttpStatusCode.OK);
        Assert.Equal(2, apiResponse.Data!.Count);
        Assert.All(apiResponse.Data, a => Assert.False(a.IsHost));
    }

    [Fact]
    public async Task GetAppointments_AsHost_ShowsIsHostTrue()
    {
        // Arrange - Create host and save the user reference
        var hostUser = await AuthenticateAsync();
        var hostProfile = await CreateIndividualProfileAsync("Host");
        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var availability = await CreateAvailabilityAsync(hostProfile.Id, startTime, startTime.AddHours(2), 60);

        // Guest books
        await AuthenticateAsync();
        var guestProfile = await CreateIndividualProfileAsync("Guest");
        await BookAppointmentAsync(hostProfile.Id, guestProfile.Id, availability.TimeSlots.First().Id);

        // Switch back to host using saved reference
        hostUser.Authenticate();

        // Act - Get appointments as host
        var response = await GetAsync(Urls.Appointments(hostProfile.Id));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<AppointmentResponse>>(HttpStatusCode.OK);
        Assert.Single(apiResponse.Data!);
        Assert.True(apiResponse.Data.First().IsHost);
    }

    [Fact]
    public async Task CancelAppointment_AsGuest_ReturnsNoContent()
    {
        // Arrange
        await AuthenticateAsync();
        var hostProfile = await CreateIndividualProfileAsync("Host");
        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var availability = await CreateAvailabilityAsync(hostProfile.Id, startTime, startTime.AddHours(2), 60);

        await AuthenticateAsync();
        var guestProfile = await CreateIndividualProfileAsync("Guest");
        var appointment = await BookAppointmentAsync(hostProfile.Id, guestProfile.Id, availability.TimeSlots.First().Id);

        // Act
        var response = await PostAsync(Urls.CancelAppointment(guestProfile.Id, appointment.Id));

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify appointment status
        var getResponse = await GetAsync(Urls.Appointment(guestProfile.Id, appointment.Id));
        var apiResponse = await getResponse.ValidateSuccessAsync<AppointmentDetailResponse>(HttpStatusCode.OK);
        Assert.Equal("Canceled", apiResponse.Data!.Status);
    }

    [Fact]
    public async Task CancelAppointment_AlreadyCanceled_ReturnsBadRequest()
    {
        // Arrange
        await AuthenticateAsync();
        var hostProfile = await CreateIndividualProfileAsync("Host");
        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var availability = await CreateAvailabilityAsync(hostProfile.Id, startTime, startTime.AddHours(2), 60);

        await AuthenticateAsync();
        var guestProfile = await CreateIndividualProfileAsync("Guest");
        var appointment = await BookAppointmentAsync(hostProfile.Id, guestProfile.Id, availability.TimeSlots.First().Id);

        // Cancel once
        await PostAsync(Urls.CancelAppointment(guestProfile.Id, appointment.Id));

        // Act - Try to cancel again
        var response = await PostAsync(Urls.CancelAppointment(guestProfile.Id, appointment.Id));

        // Assert
        await response.ValidateFailureAsync(HttpStatusCode.BadRequest, expectedErrorMessage: "already been canceled");
    }

    [Fact]
    public async Task CancelAppointment_ReleasesSlot()
    {
        // Arrange
        await AuthenticateAsync();
        var hostProfile = await CreateIndividualProfileAsync("Host");
        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var availability = await CreateAvailabilityAsync(hostProfile.Id, startTime, startTime.AddHours(2), 60);
        var slotId = availability.TimeSlots.First().Id;

        await AuthenticateAsync();
        var guestProfile = await CreateIndividualProfileAsync("Guest");
        var appointment = await BookAppointmentAsync(hostProfile.Id, guestProfile.Id, slotId);

        // Cancel
        await PostAsync(Urls.CancelAppointment(guestProfile.Id, appointment.Id));

        // Clear auth for public endpoint
        ClearAuthorizationHeader();

        // Act - Check if slot is available again
        var slots = await GetAvailableSlotsAsync(hostProfile.Id, startTime.AddHours(-1), startTime.AddHours(3));

        // Assert - Slot should be available again
        Assert.Contains(slots, s => s.Id == slotId);
    }

    [Fact]
    public async Task GetAppointments_WithStatusFilter_ReturnsFilteredResults()
    {
        // Arrange
        await AuthenticateAsync();
        var hostProfile = await CreateIndividualProfileAsync("Host");
        var startTime = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(7).AddHours(9), TimeSpan.Zero);
        var availability = await CreateAvailabilityAsync(hostProfile.Id, startTime, startTime.AddHours(3), 60);

        await AuthenticateAsync();
        var guestProfile = await CreateIndividualProfileAsync("Guest");
        var appointment1 = await BookAppointmentAsync(hostProfile.Id, guestProfile.Id, availability.TimeSlots[0].Id);
        await BookAppointmentAsync(hostProfile.Id, guestProfile.Id, availability.TimeSlots[1].Id);

        // Cancel one
        await PostAsync(Urls.CancelAppointment(guestProfile.Id, appointment1.Id));

        // Act - Filter by Scheduled only
        var response = await GetAsync(Urls.Appointments(guestProfile.Id, "Scheduled"));

        // Assert
        var apiResponse = await response.ValidateSuccessAsync<IReadOnlyList<AppointmentResponse>>(HttpStatusCode.OK);
        Assert.Single(apiResponse.Data!);
        Assert.Equal("Scheduled", apiResponse.Data.First().Status);
    }
}
