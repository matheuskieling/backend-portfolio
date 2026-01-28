using DocumentManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManager.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.Description)
            .HasMaxLength(2000);

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(d => d.FolderId);
        builder.HasIndex(d => d.OwnerId);
        builder.HasIndex(d => d.Status);

        builder.HasOne(d => d.Folder)
            .WithMany(f => f.Documents)
            .HasForeignKey(d => d.FolderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(d => d.Versions)
            .WithOne(v => v.Document)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.DocumentTags)
            .WithOne(dt => dt.Document)
            .HasForeignKey(dt => dt.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.ApprovalRequests)
            .WithOne(ar => ar.Document)
            .HasForeignKey(ar => ar.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
