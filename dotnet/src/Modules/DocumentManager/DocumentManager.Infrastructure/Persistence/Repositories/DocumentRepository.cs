using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Entities;
using DocumentManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Infrastructure.Persistence.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentManagerDbContext _context;

    public DocumentRepository(DocumentManagerDbContext context)
    {
        _context = context;
    }

    public async Task<Document?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Document?> GetByIdWithVersionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Document?> GetByIdWithTagsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Document?> GetByIdWithAllAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Include(d => d.Folder)
            .Include(d => d.Versions)
            .Include(d => d.DocumentTags)
                .ThenInclude(dt => dt.Tag)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> GetByFolderIdAsync(Guid? folderId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.FolderId == folderId)
            .OrderBy(d => d.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.OwnerId == ownerId)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> GetByStatusAsync(DocumentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .Where(d => d.Status == status)
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Document>> SearchAsync(
        string? titleFilter = null,
        Guid? folderId = null,
        Guid? ownerId = null,
        DocumentStatus? status = null,
        IEnumerable<Guid>? tagIds = null,
        int skip = 0,
        int take = 20,
        CancellationToken cancellationToken = default)
    {
        var query = BuildSearchQuery(titleFilter, folderId, ownerId, status, tagIds);

        return await query
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        string? titleFilter = null,
        Guid? folderId = null,
        Guid? ownerId = null,
        DocumentStatus? status = null,
        IEnumerable<Guid>? tagIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildSearchQuery(titleFilter, folderId, ownerId, status, tagIds);
        return await query.CountAsync(cancellationToken);
    }

    private IQueryable<Document> BuildSearchQuery(
        string? titleFilter,
        Guid? folderId,
        Guid? ownerId,
        DocumentStatus? status,
        IEnumerable<Guid>? tagIds)
    {
        var query = _context.Documents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(titleFilter))
        {
            var filter = titleFilter.Trim().ToLowerInvariant();
            query = query.Where(d => d.Title.ToLower().Contains(filter));
        }

        if (folderId.HasValue)
        {
            query = query.Where(d => d.FolderId == folderId.Value);
        }

        if (ownerId.HasValue)
        {
            query = query.Where(d => d.OwnerId == ownerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        if (tagIds != null && tagIds.Any())
        {
            var tagIdList = tagIds.ToList();
            query = query.Where(d => d.DocumentTags.Any(dt => tagIdList.Contains(dt.TagId)));
        }

        return query;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Documents.AnyAsync(d => d.Id == id, cancellationToken);
    }

    public void Add(Document document)
    {
        _context.Documents.Add(document);
    }

    public void Update(Document document)
    {
        _context.Documents.Update(document);
    }

    public void Remove(Document document)
    {
        _context.Documents.Remove(document);
    }

    public void AddVersion(DocumentVersion version)
    {
        _context.DocumentVersions.Add(version);
    }
}
