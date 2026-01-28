using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Infrastructure.Persistence.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly DocumentManagerDbContext _context;

    public FolderRepository(DocumentManagerDbContext context)
    {
        _context = context;
    }

    public async Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Folder?> GetByIdWithChildrenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Include(f => f.ChildFolders)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Folder>> GetRootFoldersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .Where(f => f.ParentFolderId == null)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Folder>> GetAllWithHierarchyAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Folders
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Folders.AnyAsync(f => f.Id == id, cancellationToken);
    }

    public void Add(Folder folder)
    {
        _context.Folders.Add(folder);
    }

    public void Update(Folder folder)
    {
        _context.Folders.Update(folder);
    }

    public void Remove(Folder folder)
    {
        _context.Folders.Remove(folder);
    }
}
