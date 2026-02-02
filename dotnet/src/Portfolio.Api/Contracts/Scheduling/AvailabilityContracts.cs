namespace Portfolio.Api.Contracts.Scheduling;

/// <summary>
/// Request model for creating a single-occurrence availability.
/// </summary>
/// <param name="StartTime">Start time of availability (ISO 8601 format).</param>
/// <param name="EndTime">End time of availability (ISO 8601 format).</param>
/// <param name="SlotDurationMinutes">Duration of each bookable slot in minutes.</param>
/// <param name="MinAdvanceBookingMinutes">Minimum minutes before a slot that booking is allowed.</param>
/// <param name="MaxAdvanceBookingDays">Maximum days in advance that booking is allowed.</param>
/// <param name="CancellationDeadlineMinutes">Minutes before appointment when cancellation is still allowed.</param>
public sealed record CreateAvailabilityRequest(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int SlotDurationMinutes,
    int MinAdvanceBookingMinutes = 60,
    int MaxAdvanceBookingDays = 30,
    int CancellationDeadlineMinutes = 60);

/// <summary>
/// Response model for availability summary.
/// </summary>
public sealed record AvailabilityResponse(
    Guid Id,
    Guid? ScheduleId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int SlotDurationMinutes,
    int TotalSlots,
    int AvailableSlots,
    int BookedSlots,
    int BlockedSlots,
    DateTime CreatedAt);

/// <summary>
/// Response model for detailed availability information.
/// </summary>
public sealed record AvailabilityDetailResponse(
    Guid Id,
    Guid? ScheduleId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int SlotDurationMinutes,
    int MinAdvanceBookingMinutes,
    int MaxAdvanceBookingDays,
    int CancellationDeadlineMinutes,
    IReadOnlyList<TimeSlotResponse> TimeSlots,
    DateTime CreatedAt);

/// <summary>
/// Response model for a time slot.
/// </summary>
public sealed record TimeSlotResponse(
    Guid Id,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status);
