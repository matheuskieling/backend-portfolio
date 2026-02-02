using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Persistence.Configurations;

public class AvailabilityConfiguration : IEntityTypeConfiguration<Availability>
{
    public void Configure(EntityTypeBuilder<Availability> builder)
    {
        builder.ToTable("availabilities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.HostProfileId)
            .IsRequired();

        builder.Property(a => a.StartTime)
            .IsRequired();

        builder.Property(a => a.EndTime)
            .IsRequired();

        builder.Property(a => a.SlotDurationMinutes)
            .IsRequired();

        builder.Property(a => a.MinAdvanceBookingMinutes)
            .IsRequired();

        builder.Property(a => a.MaxAdvanceBookingDays)
            .IsRequired();

        builder.Property(a => a.CancellationDeadlineMinutes)
            .IsRequired();

        builder.HasIndex(a => new { a.HostProfileId, a.StartTime, a.EndTime });

        builder.HasIndex(a => a.ScheduleId);

        builder.HasOne(a => a.Schedule)
            .WithMany()
            .HasForeignKey(a => a.ScheduleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.TimeSlots)
            .WithOne(t => t.Availability)
            .HasForeignKey(t => t.AvailabilityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
