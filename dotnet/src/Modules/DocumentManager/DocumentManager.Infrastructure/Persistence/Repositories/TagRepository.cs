using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Infrastructure.Persistence.Repositories;

public class TagRepository : ITagRepository
{
    private readonly DocumentManagerDbContext _context;

    public TagRepository(DocumentManagerDbContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name == normalizedName, cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.Tags
            .Where(t => idList.Contains(t.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags.AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await _context.Tags.AnyAsync(t => t.Name == normalizedName, cancellationToken);
    }

    public void Add(Tag tag)
    {
        _context.Tags.Add(tag);
    }

    public void Update(Tag tag)
    {
        _context.Tags.Update(tag);
    }

    public void Remove(Tag tag)
    {
        _context.Tags.Remove(tag);
    }
}
