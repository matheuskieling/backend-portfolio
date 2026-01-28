using Common.Domain;
using DocumentManager.Domain.ValueObjects;

namespace DocumentManager.Domain.Entities;

public sealed class DocumentVersion : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public Document Document { get; private set; } = null!;
    public int VersionNumber { get; private set; }
    public FileName FileName { get; private set; } = null!;
    public MimeType MimeType { get; private set; } = null!;
    public long FileSize { get; private set; }
    public StoragePath StoragePath { get; private set; } = null!;
    public Guid? UploadedBy { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private DocumentVersion() : base() { }

    private DocumentVersion(
        Document document,
        int versionNumber,
        FileName fileName,
        MimeType mimeType,
        long fileSize,
        StoragePath storagePath,
        Guid? uploadedBy) : base()
    {
        DocumentId = document.Id;
        Document = document;
        VersionNumber = versionNumber;
        FileName = fileName;
        MimeType = mimeType;
        FileSize = fileSize;
        StoragePath = storagePath;
        UploadedBy = uploadedBy;
        UploadedAt = DateTime.UtcNow;
    }

    public static DocumentVersion Create(
        Document document,
        int versionNumber,
        string fileName,
        string mimeType,
        long fileSize,
        string storagePath,
        Guid? uploadedBy)
    {
        var fileNameVo = FileName.Create(fileName);
        var mimeTypeVo = MimeType.Create(mimeType);
        var storagePathVo = StoragePath.Create(storagePath);

        if (fileSize <= 0)
            throw new ArgumentException("File size must be greater than zero.", nameof(fileSize));

        return new DocumentVersion(
            document,
            versionNumber,
            fileNameVo,
            mimeTypeVo,
            fileSize,
            storagePathVo,
            uploadedBy);
    }
}
