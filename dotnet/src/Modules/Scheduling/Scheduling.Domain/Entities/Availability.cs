using Common.Domain;
using Scheduling.Domain.Enums;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Domain.Entities;

public sealed class Availability : AuditableEntity, IAggregateRoot
{
    public Guid HostProfileId { get; private set; }
    public Guid? ScheduleId { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public int SlotDurationMinutes { get; private set; }
    public int MinAdvanceBookingMinutes { get; private set; }
    public int MaxAdvanceBookingDays { get; private set; }
    public int CancellationDeadlineMinutes { get; private set; }

    private readonly List<TimeSlot> _timeSlots = new();
    public IReadOnlyCollection<TimeSlot> TimeSlots => _timeSlots.AsReadOnly();

    public SchedulingProfile? HostProfile { get; private set; }
    public Schedule? Schedule { get; private set; }

    private Availability() : base() { }

    private Availability(
        Guid hostProfileId,
        Guid? scheduleId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int slotDurationMinutes,
        int minAdvanceBookingMinutes,
        int maxAdvanceBookingDays,
        int cancellationDeadlineMinutes) : base()
    {
        HostProfileId = hostProfileId;
        ScheduleId = scheduleId;
        StartTime = startTime;
        EndTime = endTime;
        SlotDurationMinutes = slotDurationMinutes;
        MinAdvanceBookingMinutes = minAdvanceBookingMinutes;
        MaxAdvanceBookingDays = maxAdvanceBookingDays;
        CancellationDeadlineMinutes = cancellationDeadlineMinutes;
    }

    public static Availability Create(
        Guid hostProfileId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int slotDurationMinutes,
        int minAdvanceBookingMinutes = 60,
        int maxAdvanceBookingDays = 30,
        int cancellationDeadlineMinutes = 60)
    {
        ValidateConfiguration(startTime, endTime, slotDurationMinutes);

        var availability = new Availability(
            hostProfileId,
            null,
            startTime,
            endTime,
            slotDurationMinutes,
            minAdvanceBookingMinutes,
            maxAdvanceBookingDays,
            cancellationDeadlineMinutes);

        availability.GenerateTimeSlots();

        return availability;
    }

    public static Availability CreateFromSchedule(
        Guid hostProfileId,
        Guid scheduleId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int slotDurationMinutes,
        int minAdvanceBookingMinutes,
        int maxAdvanceBookingDays,
        int cancellationDeadlineMinutes)
    {
        ValidateConfiguration(startTime, endTime, slotDurationMinutes);

        var availability = new Availability(
            hostProfileId,
            scheduleId,
            startTime,
            endTime,
            slotDurationMinutes,
            minAdvanceBookingMinutes,
            maxAdvanceBookingDays,
            cancellationDeadlineMinutes);

        availability.GenerateTimeSlots();

        return availability;
    }

    private void GenerateTimeSlots()
    {
        _timeSlots.Clear();

        var currentStart = StartTime;
        while (currentStart.AddMinutes(SlotDurationMinutes) <= EndTime)
        {
            var slotEnd = currentStart.AddMinutes(SlotDurationMinutes);
            var slot = TimeSlot.Create(Id, currentStart, slotEnd);
            _timeSlots.Add(slot);
            currentStart = slotEnd;
        }
    }

    public bool OverlapsWith(DateTimeOffset otherStart, DateTimeOffset otherEnd)
    {
        return StartTime < otherEnd && EndTime > otherStart;
    }

    public bool HasBookedSlots()
    {
        return _timeSlots.Any(s => s.Status == TimeSlotStatus.Booked);
    }

    public bool CanBeDeleted()
    {
        return !HasBookedSlots();
    }

    public TimeSlot? GetTimeSlot(Guid timeSlotId)
    {
        return _timeSlots.FirstOrDefault(s => s.Id == timeSlotId);
    }

    public IEnumerable<TimeSlot> GetAvailableSlots()
    {
        return _timeSlots.Where(s => s.IsAvailable);
    }

    private static void ValidateConfiguration(
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        int slotDurationMinutes)
    {
        if (endTime <= startTime)
            throw new InvalidAvailabilityException("End time must be after start time");

        if (slotDurationMinutes < 5)
            throw new InvalidAvailabilityException("Slot duration must be at least 5 minutes");

        var totalMinutes = (endTime - startTime).TotalMinutes;
        if (totalMinutes < slotDurationMinutes)
            throw new InvalidAvailabilityException("Time range must fit at least one slot");
    }
}
