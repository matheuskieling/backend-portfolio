using DocumentManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManager.Infrastructure.Persistence.Configurations;

public class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("approval_steps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StepOrder)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.RequiredRole)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(s => new { s.WorkflowId, s.StepOrder })
            .IsUnique();
    }
}
