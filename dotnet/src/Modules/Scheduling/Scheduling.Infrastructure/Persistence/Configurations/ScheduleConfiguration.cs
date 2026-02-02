using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Persistence.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.ToTable("schedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.ProfileId)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        var daysOfWeekComparer = new ValueComparer<DayOfWeek[]>(
            (c1, c2) => c1!.SequenceEqual(c2!),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToArray());

        builder.Property(s => s.DaysOfWeek)
            .HasConversion(
                days => string.Join(",", days.Select(d => (int)d)),
                value => value.Split(",", StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => (DayOfWeek)int.Parse(v))
                    .ToArray())
            .Metadata.SetValueComparer(daysOfWeekComparer);

        builder.Property(s => s.StartTimeOfDay)
            .IsRequired();

        builder.Property(s => s.EndTimeOfDay)
            .IsRequired();

        builder.Property(s => s.SlotDurationMinutes)
            .IsRequired();

        builder.Property(s => s.MinAdvanceBookingMinutes)
            .IsRequired();

        builder.Property(s => s.MaxAdvanceBookingDays)
            .IsRequired();

        builder.Property(s => s.CancellationDeadlineMinutes)
            .IsRequired();

        builder.Property(s => s.EffectiveFrom)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.HasIndex(s => new { s.ProfileId, s.Name })
            .IsUnique();

        builder.HasIndex(s => new { s.ProfileId, s.IsActive });

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
