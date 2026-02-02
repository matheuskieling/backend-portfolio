using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TimeSlotId)
            .IsRequired();

        builder.Property(a => a.HostProfileId)
            .IsRequired();

        builder.Property(a => a.GuestProfileId)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasOne(a => a.TimeSlot)
            .WithMany()
            .HasForeignKey(a => a.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.HostProfile)
            .WithMany()
            .HasForeignKey(a => a.HostProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.GuestProfile)
            .WithMany()
            .HasForeignKey(a => a.GuestProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.HostProfileId, a.Status });

        builder.HasIndex(a => new { a.GuestProfileId, a.Status });

        builder.HasIndex(a => a.TimeSlotId)
            .IsUnique();

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
