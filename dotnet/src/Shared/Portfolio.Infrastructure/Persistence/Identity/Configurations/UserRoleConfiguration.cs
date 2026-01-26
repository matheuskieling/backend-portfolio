using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Portfolio.Infrastructure.Persistence.Identity.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", "dotnet_identity");

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ur => ur.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(ur => ur.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId })
            .IsUnique();
    }
}
