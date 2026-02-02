using Identity.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Enums;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Appointments;

public sealed record GetAppointmentsQuery(
    Guid ProfileId,
    AppointmentStatus? Status = null);

public sealed record AppointmentDto(
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
    DateTimeOffset? CompletedAt);

public sealed class GetAppointmentsHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IAppointmentRepository appointmentRepository)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IReadOnlyList<AppointmentDto>> HandleAsync(
        GetAppointmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(query.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(query.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var appointments = await _appointmentRepository.GetByProfileIdAsync(
            query.ProfileId, query.Status, cancellationToken);

        return appointments.Select(a => new AppointmentDto(
            a.Id,
            a.TimeSlotId,
            a.HostProfileId,
            a.GuestProfileId,
            a.TimeSlot!.StartTime,
            a.TimeSlot!.EndTime,
            a.Status,
            a.IsHost(query.ProfileId),
            a.CreatedAt,
            a.CanceledAt,
            a.CompletedAt)).ToList();
    }
}
