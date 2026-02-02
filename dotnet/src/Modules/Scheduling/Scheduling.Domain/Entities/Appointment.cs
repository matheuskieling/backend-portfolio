using Common.Domain;
using Scheduling.Domain.Enums;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Domain.Entities;

public sealed class Appointment : AuditableEntity, IAggregateRoot
{
    public Guid TimeSlotId { get; private set; }
    public Guid HostProfileId { get; private set; }
    public Guid GuestProfileId { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }
    public Guid? CanceledBy { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public TimeSlot? TimeSlot { get; private set; }
    public SchedulingProfile? HostProfile { get; private set; }
    public SchedulingProfile? GuestProfile { get; private set; }

    private Appointment() : base() { }

    private Appointment(
        Guid timeSlotId,
        Guid hostProfileId,
        Guid guestProfileId) : base()
    {
        TimeSlotId = timeSlotId;
        HostProfileId = hostProfileId;
        GuestProfileId = guestProfileId;
        Status = AppointmentStatus.Scheduled;
    }

    public static Appointment Create(
        TimeSlot timeSlot,
        SchedulingProfile hostProfile,
        SchedulingProfile guestProfile,
        Availability availability,
        DateTimeOffset currentTime)
    {
        if (hostProfile.ExternalUserId == guestProfile.ExternalUserId)
            throw new SelfBookingNotAllowedException();

        ValidateBookingWindow(timeSlot, availability, currentTime);

        timeSlot.Book();

        return new Appointment(timeSlot.Id, hostProfile.Id, guestProfile.Id);
    }

    public void Cancel(Guid canceledByUserId, TimeSlot timeSlot, int cancellationDeadlineMinutes, DateTimeOffset currentTime)
    {
        if (Status == AppointmentStatus.Canceled)
            throw new AppointmentAlreadyCanceledException(Id);

        if (Status == AppointmentStatus.Completed)
            throw new AppointmentAlreadyCompletedException(Id);

        var deadline = timeSlot.StartTime.AddMinutes(-cancellationDeadlineMinutes);
        if (currentTime > deadline)
            throw new CancellationDeadlinePassedException(cancellationDeadlineMinutes);

        Status = AppointmentStatus.Canceled;
        CanceledAt = currentTime;
        CanceledBy = canceledByUserId;

        timeSlot.Release();

        SetUpdated();
    }

    public void Complete(DateTimeOffset currentTime)
    {
        if (Status == AppointmentStatus.Canceled)
            throw new AppointmentAlreadyCanceledException(Id);

        if (Status == AppointmentStatus.Completed)
            throw new AppointmentAlreadyCompletedException(Id);

        Status = AppointmentStatus.Completed;
        CompletedAt = currentTime;

        SetUpdated();
    }

    public bool IsParticipant(Guid profileId)
    {
        return HostProfileId == profileId || GuestProfileId == profileId;
    }

    public bool IsHost(Guid profileId)
    {
        return HostProfileId == profileId;
    }

    public bool IsGuest(Guid profileId)
    {
        return GuestProfileId == profileId;
    }

    private static void ValidateBookingWindow(
        TimeSlot timeSlot,
        Availability availability,
        DateTimeOffset currentTime)
    {
        var minBookingTime = currentTime.AddMinutes(availability.MinAdvanceBookingMinutes);
        if (timeSlot.StartTime < minBookingTime)
            throw BookingWindowViolationException.TooSoon(availability.MinAdvanceBookingMinutes);

        var maxBookingTime = currentTime.AddDays(availability.MaxAdvanceBookingDays);
        if (timeSlot.StartTime > maxBookingTime)
            throw BookingWindowViolationException.TooFarInFuture(availability.MaxAdvanceBookingDays);
    }
}
