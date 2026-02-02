using Identity.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Enums;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Appointments;

public sealed record GetAppointmentByIdQuery(Guid ProfileId, Guid AppointmentId);

public sealed record GetAppointmentByIdResult(
    Guid Id,
    Guid TimeSlotId,
    Guid HostProfileId,
    Guid GuestProfileId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    AppointmentStatus Status,
    bool IsHost,
    DateTime CreatedAt,
    DateTimeOffset? CanceledAt,
    Guid? CanceledBy,
    DateTimeOffset? CompletedAt);

public sealed class GetAppointmentByIdHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentByIdHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IAppointmentRepository appointmentRepository)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<GetAppointmentByIdResult> HandleAsync(
        GetAppointmentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(query.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(query.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(query.AppointmentId, cancellationToken)
            ?? throw new AppointmentNotFoundException(query.AppointmentId);

        if (!appointment.IsParticipant(query.ProfileId))
            throw UnauthorizedSchedulingAccessException.NotAppointmentParticipant();

        return new GetAppointmentByIdResult(
            appointment.Id,
            appointment.TimeSlotId,
            appointment.HostProfileId,
            appointment.GuestProfileId,
            appointment.TimeSlot!.StartTime,
            appointment.TimeSlot!.EndTime,
            appointment.Status,
            appointment.IsHost(query.ProfileId),
            appointment.CreatedAt,
            appointment.CanceledAt,
            appointment.CanceledBy,
            appointment.CompletedAt);
    }
}
