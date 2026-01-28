using DocumentManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManager.Infrastructure.Persistence.Configurations;

public class ApprovalDecisionConfiguration : IEntityTypeConfiguration<ApprovalDecision>
{
    public void Configure(EntityTypeBuilder<ApprovalDecision> builder)
    {
        builder.ToTable("approval_decisions");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Decision)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(d => d.Comment)
            .HasMaxLength(2000);

        builder.HasIndex(d => d.DecidedBy);

        builder.HasOne(d => d.Step)
            .WithMany()
            .HasForeignKey(d => d.StepId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
