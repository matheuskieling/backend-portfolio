using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Schedules;

public sealed record CreateScheduleCommand(
    Guid ProfileId,
    string Name,
    DayOfWeek[] DaysOfWeek,
    TimeOnly StartTimeOfDay,
    TimeOnly EndTimeOfDay,
    int SlotDurationMinutes,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveUntil,
    int MinAdvanceBookingMinutes = 60,
    int MaxAdvanceBookingDays = 30,
    int CancellationDeadlineMinutes = 60);

public sealed record CreateScheduleResult(
    Guid Id,
    string Name,
    DayOfWeek[] DaysOfWeek,
    TimeOnly StartTimeOfDay,
    TimeOnly EndTimeOfDay,
    int SlotDurationMinutes,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveUntil,
    bool IsActive);

public sealed class CreateScheduleHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public CreateScheduleHandler(
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

    public async Task<CreateScheduleResult> HandleAsync(
        CreateScheduleCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(command.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var nameExists = await _scheduleRepository.ExistsNameAsync(command.ProfileId, command.Name, cancellationToken);
        if (nameExists)
            throw new InvalidScheduleConfigurationException($"A schedule with name '{command.Name}' already exists for this profile");

        var schedule = Schedule.Create(
            command.ProfileId,
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

        _scheduleRepository.Add(schedule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateScheduleResult(
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
