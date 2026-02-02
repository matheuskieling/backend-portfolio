namespace Portfolio.Api.Contracts.Scheduling;

/// <summary>
/// Request model for creating a recurring schedule.
/// </summary>
/// <param name="Name">Name of the schedule (e.g., "Morning Hours").</param>
/// <param name="DaysOfWeek">Days when the schedule is active (0=Sunday through 6=Saturday).</param>
/// <param name="StartTimeOfDay">Daily start time in HH:mm format.</param>
/// <param name="EndTimeOfDay">Daily end time in HH:mm format.</param>
/// <param name="SlotDurationMinutes">Duration of each bookable slot in minutes.</param>
/// <param name="EffectiveFrom">Date when the schedule becomes effective (yyyy-MM-dd).</param>
/// <param name="EffectiveUntil">Optional end date for the schedule (yyyy-MM-dd).</param>
/// <param name="MinAdvanceBookingMinutes">Minimum minutes before a slot that booking is allowed.</param>
/// <param name="MaxAdvanceBookingDays">Maximum days in advance that booking is allowed.</param>
/// <param name="CancellationDeadlineMinutes">Minutes before appointment when cancellation is still allowed.</param>
public sealed record CreateScheduleRequest(
    string Name,
    int[] DaysOfWeek,
    string StartTimeOfDay,
    string EndTimeOfDay,
    int SlotDurationMinutes,
    string EffectiveFrom,
    string? EffectiveUntil,
    int MinAdvanceBookingMinutes = 60,
    int MaxAdvanceBookingDays = 30,
    int CancellationDeadlineMinutes = 60);

/// <summary>
/// Request model for updating a recurring schedule.
/// </summary>
public sealed record UpdateScheduleRequest(
    string Name,
    int[] DaysOfWeek,
    string StartTimeOfDay,
    string EndTimeOfDay,
    int SlotDurationMinutes,
    string EffectiveFrom,
    string? EffectiveUntil,
    int MinAdvanceBookingMinutes,
    int MaxAdvanceBookingDays,
    int CancellationDeadlineMinutes);

/// <summary>
/// Request model for generating availabilities from a schedule.
/// </summary>
/// <param name="FromDate">Start date for generation (yyyy-MM-dd).</param>
/// <param name="ToDate">End date for generation (yyyy-MM-dd).</param>
public sealed record GenerateAvailabilitiesRequest(
    string FromDate,
    string ToDate);

/// <summary>
/// Response model for a schedule.
/// </summary>
public sealed record ScheduleResponse(
    Guid Id,
    string Name,
    int[] DaysOfWeek,
    string StartTimeOfDay,
    string EndTimeOfDay,
    int SlotDurationMinutes,
    string EffectiveFrom,
    string? EffectiveUntil,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>
/// Response model for detailed schedule information.
/// </summary>
public sealed record ScheduleDetailResponse(
    Guid Id,
    string Name,
    int[] DaysOfWeek,
    string StartTimeOfDay,
    string EndTimeOfDay,
    int SlotDurationMinutes,
    int MinAdvanceBookingMinutes,
    int MaxAdvanceBookingDays,
    int CancellationDeadlineMinutes,
    string EffectiveFrom,
    string? EffectiveUntil,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>
/// Response model for generated availabilities.
/// </summary>
public sealed record GenerateAvailabilitiesResponse(
    int GeneratedCount,
    int SkippedCount,
    IReadOnlyList<GeneratedAvailabilityInfo> Availabilities);

/// <summary>
/// Information about a generated availability.
/// </summary>
public sealed record GeneratedAvailabilityInfo(
    Guid Id,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int SlotCount);
