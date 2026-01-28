using DocumentManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManager.Infrastructure.Persistence.Configurations;

public class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
    {
        builder.ToTable("approval_requests");

        builder.HasKey(ar => ar.Id);

        builder.Property(ar => ar.CurrentStepOrder)
            .IsRequired();

        builder.Property(ar => ar.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(ar => ar.RequestedBy);
        builder.HasIndex(ar => ar.Status);
        builder.HasIndex(ar => ar.DocumentId);

        builder.HasOne(ar => ar.Workflow)
            .WithMany()
            .HasForeignKey(ar => ar.WorkflowId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ar => ar.Decisions)
            .WithOne(d => d.ApprovalRequest)
            .HasForeignKey(d => d.ApprovalRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(ar => !ar.IsDeleted);
    }
}
