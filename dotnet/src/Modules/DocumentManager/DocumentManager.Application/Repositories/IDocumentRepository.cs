using DocumentManager.Domain.Entities;
using DocumentManager.Domain.Enums;

namespace DocumentManager.Application.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdWithVersionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Document?> GetByIdWithAllAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetByFolderIdAsync(Guid? folderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> GetByStatusAsync(DocumentStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Document>> SearchAsync(
        string? titleFilter = null,
        Guid? folderId = null,
        Guid? ownerId = null,
        DocumentStatus? status = null,
        IEnumerable<Guid>? tagIds = null,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        string? titleFilter = null,
        Guid? folderId = null,
        Guid? ownerId = null,
        DocumentStatus? status = null,
        IEnumerable<Guid>? tagIds = null,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Document document);
    void Update(Document document);
    void Remove(Document document);
}
