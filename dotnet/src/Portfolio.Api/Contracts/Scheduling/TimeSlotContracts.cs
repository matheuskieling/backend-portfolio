namespace Portfolio.Api.Contracts.Scheduling;

/// <summary>
/// Request model for blocking multiple time slots.
/// </summary>
/// <param name="SlotIds">List of time slot IDs to block.</param>
public sealed record BlockSlotsRequest(IReadOnlyList<Guid> SlotIds);

/// <summary>
/// Request model for unblocking multiple time slots.
/// </summary>
/// <param name="SlotIds">List of time slot IDs to unblock.</param>
public sealed record UnblockSlotsRequest(IReadOnlyList<Guid> SlotIds);

/// <summary>
/// Response model for block/unblock operations.
/// </summary>
public sealed record BlockUnblockResponse(
    int ProcessedCount,
    IReadOnlyList<Guid> ProcessedSlotIds);

/// <summary>
/// Response model for an available time slot (public view).
/// </summary>
public sealed record AvailableSlotResponse(
    Guid Id,
    Guid AvailabilityId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);
