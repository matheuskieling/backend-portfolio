using Scheduling.Application.Services;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Services;

public class AvailabilityGeneratorService : IAvailabilityGeneratorService
{
    public Task<IReadOnlyList<Availability>> GenerateFromScheduleAsync(
        Schedule schedule,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
    {
        var availabilities = new List<Availability>();

        var currentDate = fromDate;
        while (currentDate <= toDate)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (schedule.IsEffectiveOn(currentDate))
            {
                var startDateTime = currentDate.ToDateTime(schedule.StartTimeOfDay, DateTimeKind.Utc);
                var endDateTime = currentDate.ToDateTime(schedule.EndTimeOfDay, DateTimeKind.Utc);

                var availability = Availability.CreateFromSchedule(
                    schedule.ProfileId,
                    schedule.Id,
                    new DateTimeOffset(startDateTime, TimeSpan.Zero),
                    new DateTimeOffset(endDateTime, TimeSpan.Zero),
                    schedule.SlotDurationMinutes,
                    schedule.MinAdvanceBookingMinutes,
                    schedule.MaxAdvanceBookingDays,
                    schedule.CancellationDeadlineMinutes);

                availabilities.Add(availability);
            }

            currentDate = currentDate.AddDays(1);
        }

        return Task.FromResult<IReadOnlyList<Availability>>(availabilities);
    }
}
