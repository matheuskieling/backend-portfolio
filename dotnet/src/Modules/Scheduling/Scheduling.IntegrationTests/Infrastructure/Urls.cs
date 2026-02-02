using System.Web;

namespace Scheduling.IntegrationTests.Infrastructure;

public static class Urls
{
    private const string Base = "/api/scheduling";

    // Profiles
    public const string Profiles = $"{Base}/profiles";
    public const string MyProfiles = $"{Profiles}/me";
    public static string Profile(Guid id) => $"{Profiles}/{id}";

    // Availabilities
    public static string Availabilities(Guid profileId) => $"{Profile(profileId)}/availabilities";
    public static string Availabilities(Guid profileId, string from, string to) =>
        $"{Availabilities(profileId)}?from={from}&to={to}";
    public static string Availability(Guid profileId, Guid availabilityId) =>
        $"{Availabilities(profileId)}/{availabilityId}";

    // Time Slots
    public static string AvailableSlots(Guid profileId, string from, string to) =>
        $"{Profile(profileId)}/slots?from={from}&to={to}";
    public static string BlockSlots(Guid profileId) => $"{Profile(profileId)}/slots/block";
    public static string UnblockSlots(Guid profileId) => $"{Profile(profileId)}/slots/unblock";

    // Schedules
    public static string Schedules(Guid profileId) => $"{Profile(profileId)}/schedules";
    public static string Schedule(Guid profileId, Guid scheduleId) => $"{Schedules(profileId)}/{scheduleId}";
    public static string PauseSchedule(Guid profileId, Guid scheduleId) => $"{Schedule(profileId, scheduleId)}/pause";
    public static string ResumeSchedule(Guid profileId, Guid scheduleId) => $"{Schedule(profileId, scheduleId)}/resume";
    public static string GenerateAvailabilities(Guid profileId, Guid scheduleId) =>
        $"{Schedule(profileId, scheduleId)}/generate";

    // Appointments
    public static string Appointments(Guid profileId) => $"{Profile(profileId)}/appointments";
    public static string Appointments(Guid profileId, string status) => $"{Appointments(profileId)}?status={status}";
    public static string Appointment(Guid profileId, Guid appointmentId) =>
        $"{Appointments(profileId)}/{appointmentId}";
    public static string CancelAppointment(Guid profileId, Guid appointmentId) =>
        $"{Appointment(profileId, appointmentId)}/cancel";

    // Helper for formatting DateTimeOffset for URL query parameters
    public static string FormatTime(DateTimeOffset time) => HttpUtility.UrlEncode(time.ToString("o"));
}
