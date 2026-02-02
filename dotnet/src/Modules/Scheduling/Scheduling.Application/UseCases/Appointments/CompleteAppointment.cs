using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Appointments;

public sealed record CompleteAppointmentCommand(Guid ProfileId, Guid AppointmentId);

public sealed class CompleteAppointmentHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public CompleteAppointmentHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IAppointmentRepository appointmentRepository,
        ISchedulingUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        CompleteAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(command.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(command.AppointmentId, cancellationToken)
            ?? throw new AppointmentNotFoundException(command.AppointmentId);

        if (!appointment.IsHost(command.ProfileId))
            throw new UnauthorizedSchedulingAccessException("Only the host can mark an appointment as completed");

        var currentTime = DateTimeOffset.UtcNow;
        appointment.Complete(currentTime);

        _appointmentRepository.Update(appointment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
