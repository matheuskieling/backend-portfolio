using Common.Domain;
using DocumentManager.Domain.Enums;
using DocumentManager.Domain.Exceptions;

namespace DocumentManager.Domain.Entities;

public sealed class Document : AuditableEntity, IAggregateRoot
{
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public DocumentStatus Status { get; private set; }
    public int CurrentVersionNumber { get; private set; }
    public Guid? FolderId { get; private set; }
    public Folder? Folder { get; private set; }
    public Guid OwnerId { get; private set; }

    private readonly List<DocumentVersion> _versions = new();
    public IReadOnlyCollection<DocumentVersion> Versions => _versions.AsReadOnly();

    private readonly List<DocumentTag> _documentTags = new();
    public IReadOnlyCollection<DocumentTag> DocumentTags => _documentTags.AsReadOnly();

    private readonly List<ApprovalRequest> _approvalRequests = new();
    public IReadOnlyCollection<ApprovalRequest> ApprovalRequests => _approvalRequests.AsReadOnly();

    private Document() : base() { }

    private Document(string title, string? description, Guid? folderId, Guid ownerId) : base()
    {
        Title = title;
        Description = description;
        Status = DocumentStatus.Draft;
        CurrentVersionNumber = 0;
        FolderId = folderId;
        OwnerId = ownerId;
    }

    public static Document Create(string title, Guid ownerId, string? description = null, Guid? folderId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Document title cannot be empty.", nameof(title));

        return new Document(title.Trim(), description?.Trim(), folderId, ownerId);
    }

    public void Update(string title, string? description)
    {
        EnsureIsDraft();

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Document title cannot be empty.", nameof(title));

        Title = title.Trim();
        Description = description?.Trim();
        SetUpdated();
    }

    public void MoveToFolder(Guid? folderId)
    {
        EnsureIsDraft();
        FolderId = folderId;
        SetUpdated();
    }

    public DocumentVersion AddVersion(
        string fileName,
        string mimeType,
        long fileSize,
        string storagePath,
        Guid? uploadedBy)
    {
        EnsureIsDraft();

        CurrentVersionNumber++;
        var version = DocumentVersion.Create(
            this,
            CurrentVersionNumber,
            fileName,
            mimeType,
            fileSize,
            storagePath,
            uploadedBy);

        _versions.Add(version);
        SetUpdated();

        return version;
    }

    public DocumentVersion? GetCurrentVersion()
    {
        return _versions.FirstOrDefault(v => v.VersionNumber == CurrentVersionNumber);
    }

    public DocumentVersion? GetVersion(int versionNumber)
    {
        return _versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
    }

    public void AddTag(Tag tag)
    {
        if (_documentTags.Any(dt => dt.TagId == tag.Id))
            return;

        var documentTag = DocumentTag.Create(this, tag);
        _documentTags.Add(documentTag);
        SetUpdated();
    }

    public void RemoveTag(Tag tag)
    {
        var documentTag = _documentTags.FirstOrDefault(dt => dt.TagId == tag.Id);
        if (documentTag is not null)
        {
            _documentTags.Remove(documentTag);
            SetUpdated();
        }
    }

    public bool HasTag(Guid tagId)
    {
        return _documentTags.Any(dt => dt.TagId == tagId);
    }

    public IEnumerable<string> GetTagNames()
    {
        return _documentTags.Select(dt => dt.Tag.Name);
    }

    public void SubmitForApproval()
    {
        EnsureIsDraft();

        if (CurrentVersionNumber == 0)
            throw new DocumentHasNoVersionsException(Id);

        Status = DocumentStatus.PendingApproval;
        SetUpdated();
    }

    public void Approve()
    {
        if (Status != DocumentStatus.PendingApproval)
            throw new InvalidDocumentStateException(Id, Status, DocumentStatus.PendingApproval);

        Status = DocumentStatus.Approved;
        SetUpdated();
    }

    public void Reject()
    {
        if (Status != DocumentStatus.PendingApproval)
            throw new InvalidDocumentStateException(Id, Status, DocumentStatus.PendingApproval);

        Status = DocumentStatus.Rejected;
        SetUpdated();
    }

    public void RevertToDraft()
    {
        if (Status == DocumentStatus.Approved)
            throw new InvalidDocumentStateException(Id, "Approved documents cannot be reverted to draft.");

        Status = DocumentStatus.Draft;
        SetUpdated();
    }

    public bool IsOwnedBy(Guid userId)
    {
        return OwnerId == userId;
    }

    public void EnsureIsDraft()
    {
        if (Status != DocumentStatus.Draft)
            throw new DocumentNotInDraftException(Id);
    }

    public void EnsureCanBeModifiedBy(Guid userId)
    {
        EnsureIsDraft();

        if (!IsOwnedBy(userId))
            throw new UnauthorizedDocumentAccessException(Id, userId);
    }
}
