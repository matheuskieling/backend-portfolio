using DocumentManager.Domain.Entities;
using DocumentManager.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManager.Infrastructure.Persistence.Configurations;

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("document_versions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.VersionNumber)
            .IsRequired();

        builder.Property(v => v.FileName)
            .HasConversion(
                fileName => fileName.Value,
                value => FileName.Create(value))
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(v => v.MimeType)
            .HasConversion(
                mimeType => mimeType.Value,
                value => MimeType.Create(value))
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(v => v.FileSize)
            .IsRequired();

        builder.Property(v => v.StoragePath)
            .HasConversion(
                storagePath => storagePath.Value,
                value => StoragePath.Create(value))
            .IsRequired()
            .HasMaxLength(1024);

        builder.HasIndex(v => v.UploadedBy);

        builder.HasIndex(v => new { v.DocumentId, v.VersionNumber })
            .IsUnique();
    }
}
