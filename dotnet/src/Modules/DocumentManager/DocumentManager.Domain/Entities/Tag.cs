using Common.Domain;

namespace DocumentManager.Domain.Entities;

public sealed class Tag : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = null!;

    private readonly List<DocumentTag> _documentTags = new();
    public IReadOnlyCollection<DocumentTag> DocumentTags => _documentTags.AsReadOnly();

    private Tag() : base() { }

    private Tag(string name) : base()
    {
        Name = name;
    }

    public static Tag Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name cannot be empty.", nameof(name));

        var normalizedName = name.Trim().ToLowerInvariant();

        if (normalizedName.Length > 50)
            throw new ArgumentException("Tag name cannot exceed 50 characters.", nameof(name));

        return new Tag(normalizedName);
    }
}
