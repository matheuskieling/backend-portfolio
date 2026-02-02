using Identity.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Schedules;

public sealed record GetScheduleByIdQuery(Guid ProfileId, Guid ScheduleId);

public sealed record GetScheduleByIdResult(
    Guid Id,
    string Name,
    DayOfWeek[] DaysOfWeek,
    TimeOnly StartTimeOfDay,
    TimeOnly EndTimeOfDay,
    int SlotDurationMinutes,
    int MinAdvanceBookingMinutes,
    int MaxAdvanceBookingDays,
    int CancellationDeadlineMinutes,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveUntil,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed class GetScheduleByIdHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IScheduleRepository _scheduleRepository;

    public GetScheduleByIdHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IScheduleRepository scheduleRepository)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<GetScheduleByIdResult> HandleAsync(
        GetScheduleByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(query.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(query.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var schedule = await _scheduleRepository.GetByIdAsync(query.ScheduleId, cancellationToken)
            ?? throw new ScheduleNotFoundException(query.ScheduleId);

        if (schedule.ProfileId != query.ProfileId)
            throw new ScheduleNotFoundException(query.ScheduleId);

        return new GetScheduleByIdResult(
            schedule.Id,
            schedule.Name,
            schedule.DaysOfWeek,
            schedule.StartTimeOfDay,
            schedule.EndTimeOfDay,
            schedule.SlotDurationMinutes,
            schedule.MinAdvanceBookingMinutes,
            schedule.MaxAdvanceBookingDays,
            schedule.CancellationDeadlineMinutes,
            schedule.EffectiveFrom,
            schedule.EffectiveUntil,
            schedule.IsActive,
            schedule.CreatedAt,
            schedule.UpdatedAt);
    }
}
