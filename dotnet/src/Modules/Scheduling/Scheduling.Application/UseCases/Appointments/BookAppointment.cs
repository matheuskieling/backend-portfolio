using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Appointments;

public sealed record BookAppointmentCommand(
    Guid HostProfileId,
    Guid GuestProfileId,
    Guid TimeSlotId);

public sealed record BookAppointmentResult(
    Guid Id,
    Guid TimeSlotId,
    Guid HostProfileId,
    Guid GuestProfileId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    AppointmentStatus Status,
    DateTime CreatedAt);

public sealed class BookAppointmentHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public BookAppointmentHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        ITimeSlotRepository timeSlotRepository,
        IAvailabilityRepository availabilityRepository,
        IAppointmentRepository appointmentRepository,
        ISchedulingUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _timeSlotRepository = timeSlotRepository;
        _availabilityRepository = availabilityRepository;
        _appointmentRepository = appointmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BookAppointmentResult> HandleAsync(
        BookAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var hostProfile = await _profileRepository.GetByIdAsync(command.HostProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.HostProfileId);

        var guestProfile = await _profileRepository.GetByIdAsync(command.GuestProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.GuestProfileId);

        if (!guestProfile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var timeSlot = await _timeSlotRepository.GetByIdWithAvailabilityAsync(command.TimeSlotId, cancellationToken)
            ?? throw new TimeSlotNotFoundException(command.TimeSlotId);

        if (timeSlot.Availability?.HostProfileId != command.HostProfileId)
            throw new TimeSlotNotFoundException(command.TimeSlotId);

        var availability = timeSlot.Availability!;
        var currentTime = DateTimeOffset.UtcNow;

        var appointment = Appointment.Create(
            timeSlot,
            hostProfile,
            guestProfile,
            availability,
            currentTime);

        _appointmentRepository.Add(appointment);
        _timeSlotRepository.Update(timeSlot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BookAppointmentResult(
            appointment.Id,
            appointment.TimeSlotId,
            appointment.HostProfileId,
            appointment.GuestProfileId,
            timeSlot.StartTime,
            timeSlot.EndTime,
            appointment.Status,
            appointment.CreatedAt);
    }
}
