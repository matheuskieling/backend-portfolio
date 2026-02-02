using Common.Domain;
using Scheduling.Domain.Enums;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Domain.Entities;

public sealed class TimeSlot : BaseEntity
{
    public Guid AvailabilityId { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public TimeSlotStatus Status { get; private set; }

    public Availability? Availability { get; private set; }

    private TimeSlot() : base() { }

    private TimeSlot(
        Guid availabilityId,
        DateTimeOffset startTime,
        DateTimeOffset endTime) : base()
    {
        AvailabilityId = availabilityId;
        StartTime = startTime;
        EndTime = endTime;
        Status = TimeSlotStatus.Available;
    }

    internal static TimeSlot Create(
        Guid availabilityId,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        return new TimeSlot(availabilityId, startTime, endTime);
    }

    public void Book()
    {
        if (Status != TimeSlotStatus.Available)
            throw new TimeSlotNotAvailableException(Id, Status);

        Status = TimeSlotStatus.Booked;
    }

    public void Release()
    {
        if (Status == TimeSlotStatus.Booked)
        {
            Status = TimeSlotStatus.Available;
        }
    }

    public void Block()
    {
        if (Status == TimeSlotStatus.Booked)
            throw new CannotBlockBookedSlotException(Id);

        if (Status == TimeSlotStatus.Blocked)
            throw new TimeSlotAlreadyBlockedException(Id);

        Status = TimeSlotStatus.Blocked;
    }

    public void Unblock()
    {
        if (Status == TimeSlotStatus.Blocked)
        {
            Status = TimeSlotStatus.Available;
        }
    }

    public void Cancel()
    {
        Status = TimeSlotStatus.Canceled;
    }

    public bool IsAvailable => Status == TimeSlotStatus.Available;
    public bool IsBooked => Status == TimeSlotStatus.Booked;
    public bool IsBlocked => Status == TimeSlotStatus.Blocked;
}
