using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Schedules;

public sealed record UpdateScheduleCommand(
    Guid ProfileId,
    Guid ScheduleId,
    string Name,
    DayOfWeek[] DaysOfWeek,
    TimeOnly StartTimeOfDay,
    TimeOnly EndTimeOfDay,
    int SlotDurationMinutes,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveUntil,
    int MinAdvanceBookingMinutes,
    int MaxAdvanceBookingDays,
    int CancellationDeadlineMinutes);

public sealed record UpdateScheduleResult(
    Guid Id,
    string Name,
    DayOfWeek[] DaysOfWeek,
    TimeOnly StartTimeOfDay,
    TimeOnly EndTimeOfDay,
    int SlotDurationMinutes,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveUntil,
    bool IsActive);

public sealed class UpdateScheduleHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public UpdateScheduleHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IScheduleRepository scheduleRepository,
        ISchedulingUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _scheduleRepository = scheduleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateScheduleResult> HandleAsync(
        UpdateScheduleCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(command.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var schedule = await _scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken)
            ?? throw new ScheduleNotFoundException(command.ScheduleId);

        if (schedule.ProfileId != command.ProfileId)
            throw new ScheduleNotFoundException(command.ScheduleId);

        var nameExists = await _scheduleRepository.ExistsNameExcludingAsync(
            command.ProfileId, command.Name, command.ScheduleId, cancellationToken);
        if (nameExists)
            throw new InvalidScheduleConfigurationException($"A schedule with name '{command.Name}' already exists for this profile");

        schedule.Update(
            command.Name,
            command.DaysOfWeek,
            command.StartTimeOfDay,
            command.EndTimeOfDay,
            command.SlotDurationMinutes,
            command.EffectiveFrom,
            command.EffectiveUntil,
            command.MinAdvanceBookingMinutes,
            command.MaxAdvanceBookingDays,
            command.CancellationDeadlineMinutes);

        _scheduleRepository.Update(schedule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateScheduleResult(
            schedule.Id,
            schedule.Name,
            schedule.DaysOfWeek,
            schedule.StartTimeOfDay,
            schedule.EndTimeOfDay,
            schedule.SlotDurationMinutes,
            schedule.EffectiveFrom,
            schedule.EffectiveUntil,
            schedule.IsActive);
    }
}
