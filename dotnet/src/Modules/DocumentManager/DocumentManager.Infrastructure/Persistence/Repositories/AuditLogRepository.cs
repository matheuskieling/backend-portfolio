using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Infrastructure.Persistence.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly DocumentManagerDbContext _context;

    public AuditLogRepository(DocumentManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.PerformedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByPerformedByAsync(Guid performedBy, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(a => a.PerformedBy == performedBy)
            .OrderByDescending(a => a.PerformedAt)
            .ToListAsync(cancellationToken);
    }

    public void Add(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
    }
}
