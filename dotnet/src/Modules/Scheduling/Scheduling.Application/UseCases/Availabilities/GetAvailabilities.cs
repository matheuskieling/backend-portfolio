using Identity.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Availabilities;

public sealed record GetAvailabilitiesQuery(
    Guid ProfileId,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null);

public sealed record AvailabilityDto(
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

public sealed class GetAvailabilitiesHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IAvailabilityRepository _availabilityRepository;

    public GetAvailabilitiesHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IAvailabilityRepository availabilityRepository)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _availabilityRepository = availabilityRepository;
    }

    public async Task<IReadOnlyList<AvailabilityDto>> HandleAsync(
        GetAvailabilitiesQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(query.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(query.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var availabilities = await _availabilityRepository.GetByProfileIdAsync(
            query.ProfileId, query.From, query.To, cancellationToken);

        return availabilities.Select(a => new AvailabilityDto(
            a.Id,
            a.ScheduleId,
            a.StartTime,
            a.EndTime,
            a.SlotDurationMinutes,
            a.TimeSlots.Count,
            a.TimeSlots.Count(s => s.IsAvailable),
            a.TimeSlots.Count(s => s.IsBooked),
            a.TimeSlots.Count(s => s.IsBlocked),
            a.CreatedAt)).ToList();
    }
}
