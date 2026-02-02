namespace Portfolio.Api.Contracts.Scheduling;

/// <summary>
/// Request model for creating a scheduling profile.
/// </summary>
/// <param name="Type">The profile type: "Individual" or "Business".</param>
/// <param name="DisplayName">Optional display name for the profile.</param>
/// <param name="BusinessName">Required for Business profiles, unique per user.</param>
public sealed record CreateProfileRequest(
    string Type,
    string? DisplayName,
    string? BusinessName);

/// <summary>
/// Response model for a scheduling profile.
/// </summary>
/// <param name="Id">The unique identifier of the profile.</param>
/// <param name="Type">The profile type.</param>
/// <param name="DisplayName">The display name of the profile.</param>
/// <param name="BusinessName">The business name (for Business profiles).</param>
/// <param name="CreatedAt">When the profile was created.</param>
public sealed record ProfileResponse(
    Guid Id,
    string Type,
    string? DisplayName,
    string? BusinessName,
    DateTime CreatedAt);
