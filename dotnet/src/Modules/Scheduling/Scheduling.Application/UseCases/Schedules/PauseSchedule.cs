using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Schedules;

public sealed record PauseScheduleCommand(Guid ProfileId, Guid ScheduleId);

public sealed class PauseScheduleHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public PauseScheduleHandler(
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

    public async Task HandleAsync(
        PauseScheduleCommand command,
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

        schedule.Pause();

        _scheduleRepository.Update(schedule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
