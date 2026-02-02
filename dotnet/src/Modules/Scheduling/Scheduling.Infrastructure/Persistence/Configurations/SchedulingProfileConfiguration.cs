using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Infrastructure.Persistence.Configurations;

public class SchedulingProfileConfiguration : IEntityTypeConfiguration<SchedulingProfile>
{
    public void Configure(EntityTypeBuilder<SchedulingProfile> builder)
    {
        builder.ToTable("scheduling_profiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ExternalUserId)
            .IsRequired();

        builder.Property(p => p.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(p => p.DisplayName)
            .HasMaxLength(200);

        builder.Property(p => p.BusinessName)
            .HasMaxLength(200);

        builder.HasIndex(p => p.ExternalUserId);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasMany(p => p.Schedules)
            .WithOne(s => s.Profile)
            .HasForeignKey(s => s.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Availabilities)
            .WithOne(a => a.HostProfile)
            .HasForeignKey(a => a.HostProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
