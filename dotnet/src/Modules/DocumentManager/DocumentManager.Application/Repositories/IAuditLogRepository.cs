using DocumentManager.Domain.Entities;

namespace DocumentManager.Application.Repositories;

public interface IAuditLogRepository
{
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByPerformedByAsync(Guid performedBy, CancellationToken cancellationToken = default);
    void Add(AuditLog auditLog);
}
