using Common.Domain;

namespace DocumentManager.Domain.Entities;

public sealed class DocumentTag : BaseEntity
{
    public Guid DocumentId { get; private set; }
    public Document Document { get; private set; } = null!;
    public Guid TagId { get; private set; }
    public Tag Tag { get; private set; } = null!;
    public DateTime AssignedAt { get; private set; }

    private DocumentTag() : base() { }

    private DocumentTag(Document document, Tag tag) : base()
    {
        DocumentId = document.Id;
        Document = document;
        TagId = tag.Id;
        Tag = tag;
        AssignedAt = DateTime.UtcNow;
    }

    public static DocumentTag Create(Document document, Tag tag)
    {
        return new DocumentTag(document, tag);
    }
}
