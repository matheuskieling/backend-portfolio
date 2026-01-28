using DocumentManager.Domain.Entities;

namespace DocumentManager.Application.Repositories;

public interface IFolderRepository
{
    Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Folder?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Folder>> GetRootFoldersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Folder>> GetAllWithHierarchyAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(Folder folder);
    void Update(Folder folder);
    void Remove(Folder folder);
}
