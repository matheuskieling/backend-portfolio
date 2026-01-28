using Common.Domain;

namespace DocumentManager.Domain.Entities;

public sealed class Folder : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public Guid? ParentFolderId { get; private set; }
    public Folder? ParentFolder { get; private set; }

    private readonly List<Folder> _childFolders = new();
    public IReadOnlyCollection<Folder> ChildFolders => _childFolders.AsReadOnly();

    private readonly List<Document> _documents = new();
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    private Folder() : base() { }

    private Folder(string name, Guid? parentFolderId) : base()
    {
        Name = name;
        ParentFolderId = parentFolderId;
    }

    public static Folder Create(string name, Guid? parentFolderId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Folder name cannot be empty.", nameof(name));

        return new Folder(name.Trim(), parentFolderId);
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Folder name cannot be empty.", nameof(newName));

        Name = newName.Trim();
        SetUpdated();
    }

    public void Move(Guid? newParentFolderId)
    {
        if (newParentFolderId == Id)
            throw new InvalidOperationException("A folder cannot be its own parent.");

        ParentFolderId = newParentFolderId;
        SetUpdated();
    }
}
