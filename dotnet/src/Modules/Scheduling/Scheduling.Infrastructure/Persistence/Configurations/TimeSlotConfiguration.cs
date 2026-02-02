using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Persistence.Configurations;

public class TimeSlotConfiguration : IEntityTypeConfiguration<TimeSlot>
{
    public void Configure(EntityTypeBuilder<TimeSlot> builder)
    {
        builder.ToTable("time_slots");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.AvailabilityId)
            .IsRequired();

        builder.Property(t => t.StartTime)
            .IsRequired();

        builder.Property(t => t.EndTime)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(t => new { t.AvailabilityId, t.Status, t.StartTime });

        builder.HasIndex(t => new { t.Status, t.StartTime });
    }
}
