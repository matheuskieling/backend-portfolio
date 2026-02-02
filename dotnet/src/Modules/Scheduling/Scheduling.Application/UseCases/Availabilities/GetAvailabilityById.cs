using Identity.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Availabilities;

public sealed record GetAvailabilityByIdQuery(Guid ProfileId, Guid AvailabilityId);

public sealed record GetAvailabilityByIdResult(
    Guid Id,
    Guid? ScheduleId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int SlotDurationMinutes,
    int MinAdvanceBookingMinutes,
    int MaxAdvanceBookingDays,
    int CancellationDeadlineMinutes,
    IReadOnlyList<TimeSlotDto> TimeSlots,
    DateTime CreatedAt);

public sealed class GetAvailabilityByIdHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IAvailabilityRepository _availabilityRepository;

    public GetAvailabilityByIdHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IAvailabilityRepository availabilityRepository)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _availabilityRepository = availabilityRepository;
    }

    public async Task<GetAvailabilityByIdResult> HandleAsync(
        GetAvailabilityByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(query.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(query.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var availability = await _availabilityRepository.GetByIdWithTimeSlotsAsync(query.AvailabilityId, cancellationToken)
            ?? throw new AvailabilityNotFoundException(query.AvailabilityId);

        if (availability.HostProfileId != query.ProfileId)
            throw new AvailabilityNotFoundException(query.AvailabilityId);

        return new GetAvailabilityByIdResult(
            availability.Id,
            availability.ScheduleId,
            availability.StartTime,
            availability.EndTime,
            availability.SlotDurationMinutes,
            availability.MinAdvanceBookingMinutes,
            availability.MaxAdvanceBookingDays,
            availability.CancellationDeadlineMinutes,
            availability.TimeSlots.Select(s => new TimeSlotDto(
                s.Id,
                s.StartTime,
                s.EndTime,
                s.Status.ToString())).ToList(),
            availability.CreatedAt);
    }
}
