using Common.Domain;
using Scheduling.Domain.Enums;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Domain.Entities;

public sealed class SchedulingProfile : AuditableEntity, IAggregateRoot
{
    public Guid ExternalUserId { get; private set; }
    public ProfileType Type { get; private set; }
    public string? DisplayName { get; private set; }
    public string? BusinessName { get; private set; }

    private readonly List<Schedule> _schedules = new();
    public IReadOnlyCollection<Schedule> Schedules => _schedules.AsReadOnly();

    private readonly List<Availability> _availabilities = new();
    public IReadOnlyCollection<Availability> Availabilities => _availabilities.AsReadOnly();

    private SchedulingProfile() : base() { }

    private SchedulingProfile(
        Guid externalUserId,
        ProfileType type,
        string? displayName,
        string? businessName) : base()
    {
        ExternalUserId = externalUserId;
        Type = type;
        DisplayName = displayName;
        BusinessName = businessName;
    }

    public static SchedulingProfile CreateIndividual(Guid externalUserId, string? displayName = null)
    {
        return new SchedulingProfile(
            externalUserId,
            ProfileType.Individual,
            displayName?.Trim(),
            null);
    }

    public static SchedulingProfile CreateBusiness(
        Guid externalUserId,
        string businessName,
        string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(businessName))
            throw new InvalidScheduleConfigurationException("Business name is required for business profiles");

        return new SchedulingProfile(
            externalUserId,
            ProfileType.Business,
            displayName?.Trim(),
            businessName.Trim());
    }

    public void UpdateDisplayName(string? displayName)
    {
        DisplayName = displayName?.Trim();
        SetUpdated();
    }

    public void UpdateBusinessName(string businessName)
    {
        if (Type != ProfileType.Business)
            throw new InvalidScheduleConfigurationException("Cannot update business name on non-business profile");

        if (string.IsNullOrWhiteSpace(businessName))
            throw new InvalidScheduleConfigurationException("Business name is required for business profiles");

        BusinessName = businessName.Trim();
        SetUpdated();
    }

    public bool IsOwnedBy(Guid userId) => ExternalUserId == userId;
}
