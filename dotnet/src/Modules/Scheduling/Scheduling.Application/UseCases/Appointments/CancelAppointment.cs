using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Appointments;

public sealed record CancelAppointmentCommand(Guid ProfileId, Guid AppointmentId);

public sealed class CancelAppointmentHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public CancelAppointmentHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IAppointmentRepository appointmentRepository,
        ITimeSlotRepository timeSlotRepository,
        IAvailabilityRepository availabilityRepository,
        ISchedulingUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _appointmentRepository = appointmentRepository;
        _timeSlotRepository = timeSlotRepository;
        _availabilityRepository = availabilityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        CancelAppointmentCommand command,
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

        if (!appointment.IsParticipant(command.ProfileId))
            throw UnauthorizedSchedulingAccessException.NotAppointmentParticipant();

        var timeSlot = appointment.TimeSlot!;
        var availability = await _availabilityRepository.GetByIdAsync(timeSlot.AvailabilityId, cancellationToken)
            ?? throw new AvailabilityNotFoundException(timeSlot.AvailabilityId);

        var currentTime = DateTimeOffset.UtcNow;

        appointment.Cancel(userId, timeSlot, availability.CancellationDeadlineMinutes, currentTime);

        _appointmentRepository.Update(appointment);
        _timeSlotRepository.Update(timeSlot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
