namespace Portfolio.Api.Contracts.Scheduling;

/// <summary>
/// Request model for booking an appointment.
/// </summary>
/// <param name="GuestProfileId">The profile ID of the guest booking the appointment.</param>
/// <param name="TimeSlotId">The time slot ID to book.</param>
public sealed record BookAppointmentRequest(
    Guid GuestProfileId,
    Guid TimeSlotId);

/// <summary>
/// Response model for an appointment.
/// </summary>
public sealed record AppointmentResponse(
    Guid Id,
    Guid TimeSlotId,
    Guid HostProfileId,
    Guid GuestProfileId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    bool IsHost,
    DateTime CreatedAt,
    DateTimeOffset? CanceledAt,
    DateTimeOffset? CompletedAt);

/// <summary>
/// Response model for detailed appointment information.
/// </summary>
public sealed record AppointmentDetailResponse(
    Guid Id,
    Guid TimeSlotId,
    Guid HostProfileId,
    Guid GuestProfileId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    bool IsHost,
    DateTime CreatedAt,
    DateTimeOffset? CanceledAt,
    Guid? CanceledBy,
    DateTimeOffset? CompletedAt);
