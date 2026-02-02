using Identity.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Schedules;

public sealed record GetSchedulesQuery(Guid ProfileId);

public sealed record ScheduleDto(
    Guid Id,
    string Name,
    DayOfWeek[] DaysOfWeek,
    TimeOnly StartTimeOfDay,
    TimeOnly EndTimeOfDay,
    int SlotDurationMinutes,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveUntil,
    bool IsActive,
    DateTime CreatedAt);

public sealed class GetSchedulesHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IScheduleRepository _scheduleRepository;

    public GetSchedulesHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IScheduleRepository scheduleRepository)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<IReadOnlyList<ScheduleDto>> HandleAsync(
        GetSchedulesQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(query.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(query.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var schedules = await _scheduleRepository.GetByProfileIdAsync(query.ProfileId, cancellationToken);

        return schedules.Select(s => new ScheduleDto(
            s.Id,
            s.Name,
            s.DaysOfWeek,
            s.StartTimeOfDay,
            s.EndTimeOfDay,
            s.SlotDurationMinutes,
            s.EffectiveFrom,
            s.EffectiveUntil,
            s.IsActive,
            s.CreatedAt)).ToList();
    }
}
