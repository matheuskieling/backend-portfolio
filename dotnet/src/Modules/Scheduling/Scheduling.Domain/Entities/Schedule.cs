using Common.Domain;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Domain.Entities;

public sealed class Schedule : AuditableEntity, IAggregateRoot
{
    public Guid ProfileId { get; private set; }
    public string Name { get; private set; } = null!;
    public DayOfWeek[] DaysOfWeek { get; private set; } = Array.Empty<DayOfWeek>();
    public TimeOnly StartTimeOfDay { get; private set; }
    public TimeOnly EndTimeOfDay { get; private set; }
    public int SlotDurationMinutes { get; private set; }
    public int MinAdvanceBookingMinutes { get; private set; }
    public int MaxAdvanceBookingDays { get; private set; }
    public int CancellationDeadlineMinutes { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveUntil { get; private set; }
    public bool IsActive { get; private set; }

    public SchedulingProfile? Profile { get; private set; }

    private Schedule() : base() { }

    private Schedule(
        Guid profileId,
        string name,
        DayOfWeek[] daysOfWeek,
        TimeOnly startTimeOfDay,
        TimeOnly endTimeOfDay,
        int slotDurationMinutes,
        int minAdvanceBookingMinutes,
        int maxAdvanceBookingDays,
        int cancellationDeadlineMinutes,
        DateOnly effectiveFrom,
        DateOnly? effectiveUntil) : base()
    {
        ProfileId = profileId;
        Name = name;
        DaysOfWeek = daysOfWeek;
        StartTimeOfDay = startTimeOfDay;
        EndTimeOfDay = endTimeOfDay;
        SlotDurationMinutes = slotDurationMinutes;
        MinAdvanceBookingMinutes = minAdvanceBookingMinutes;
        MaxAdvanceBookingDays = maxAdvanceBookingDays;
        CancellationDeadlineMinutes = cancellationDeadlineMinutes;
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        IsActive = true;
    }

    public static Schedule Create(
        Guid profileId,
        string name,
        DayOfWeek[] daysOfWeek,
        TimeOnly startTimeOfDay,
        TimeOnly endTimeOfDay,
        int slotDurationMinutes,
        DateOnly effectiveFrom,
        DateOnly? effectiveUntil = null,
        int minAdvanceBookingMinutes = 60,
        int maxAdvanceBookingDays = 30,
        int cancellationDeadlineMinutes = 60)
    {
        ValidateConfiguration(
            name,
            daysOfWeek,
            startTimeOfDay,
            endTimeOfDay,
            slotDurationMinutes,
            effectiveFrom,
            effectiveUntil);

        return new Schedule(
            profileId,
            name.Trim(),
            daysOfWeek.Distinct().OrderBy(d => d).ToArray(),
            startTimeOfDay,
            endTimeOfDay,
            slotDurationMinutes,
            minAdvanceBookingMinutes,
            maxAdvanceBookingDays,
            cancellationDeadlineMinutes,
            effectiveFrom,
            effectiveUntil);
    }

    public void Update(
        string name,
        DayOfWeek[] daysOfWeek,
        TimeOnly startTimeOfDay,
        TimeOnly endTimeOfDay,
        int slotDurationMinutes,
        DateOnly effectiveFrom,
        DateOnly? effectiveUntil,
        int minAdvanceBookingMinutes,
        int maxAdvanceBookingDays,
        int cancellationDeadlineMinutes)
    {
        ValidateConfiguration(
            name,
            daysOfWeek,
            startTimeOfDay,
            endTimeOfDay,
            slotDurationMinutes,
            effectiveFrom,
            effectiveUntil);

        Name = name.Trim();
        DaysOfWeek = daysOfWeek.Distinct().OrderBy(d => d).ToArray();
        StartTimeOfDay = startTimeOfDay;
        EndTimeOfDay = endTimeOfDay;
        SlotDurationMinutes = slotDurationMinutes;
        MinAdvanceBookingMinutes = minAdvanceBookingMinutes;
        MaxAdvanceBookingDays = maxAdvanceBookingDays;
        CancellationDeadlineMinutes = cancellationDeadlineMinutes;
        EffectiveFrom = effectiveFrom;
        EffectiveUntil = effectiveUntil;
        SetUpdated();
    }

    public void Pause()
    {
        IsActive = false;
        SetUpdated();
    }

    public void Resume()
    {
        IsActive = true;
        SetUpdated();
    }

    public bool IsEffectiveOn(DateOnly date)
    {
        if (!IsActive)
            return false;

        if (date < EffectiveFrom)
            return false;

        if (EffectiveUntil.HasValue && date > EffectiveUntil.Value)
            return false;

        return DaysOfWeek.Contains(date.DayOfWeek);
    }

    public int GetSlotCount()
    {
        var totalMinutes = (EndTimeOfDay - StartTimeOfDay).TotalMinutes;
        return (int)(totalMinutes / SlotDurationMinutes);
    }

    private static void ValidateConfiguration(
        string name,
        DayOfWeek[] daysOfWeek,
        TimeOnly startTimeOfDay,
        TimeOnly endTimeOfDay,
        int slotDurationMinutes,
        DateOnly effectiveFrom,
        DateOnly? effectiveUntil)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidScheduleConfigurationException("Schedule name is required");

        if (daysOfWeek.Length == 0)
            throw new InvalidScheduleConfigurationException("At least one day of week must be selected");

        if (endTimeOfDay <= startTimeOfDay)
            throw new InvalidScheduleConfigurationException("End time must be after start time");

        if (slotDurationMinutes < 5)
            throw new InvalidScheduleConfigurationException("Slot duration must be at least 5 minutes");

        var totalMinutes = (endTimeOfDay - startTimeOfDay).TotalMinutes;
        if (totalMinutes < slotDurationMinutes)
            throw new InvalidScheduleConfigurationException("Time range must fit at least one slot");

        if (effectiveUntil.HasValue && effectiveUntil.Value < effectiveFrom)
            throw new InvalidScheduleConfigurationException("Effective until date must be after effective from date");
    }
}
