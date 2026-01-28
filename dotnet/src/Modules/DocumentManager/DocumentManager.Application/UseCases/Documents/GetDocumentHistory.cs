using DocumentManager.Application.DTOs;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Exceptions;

namespace DocumentManager.Application.UseCases.Documents;

public sealed record GetDocumentHistoryQuery(Guid DocumentId);

public sealed class GetDocumentHistoryHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public GetDocumentHistoryHandler(
        IDocumentRepository documentRepository,
        IAuditLogRepository auditLogRepository)
    {
        _documentRepository = documentRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<IReadOnlyList<AuditLogDto>> HandleAsync(
        GetDocumentHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var documentExists = await _documentRepository.ExistsAsync(query.DocumentId, cancellationToken);
        if (!documentExists)
            throw new DocumentNotFoundException(query.DocumentId);

        var auditLogs = await _auditLogRepository.GetByEntityAsync(
            "Document",
            query.DocumentId,
            cancellationToken);

        return auditLogs
            .OrderByDescending(log => log.PerformedAt)
            .Select(log => new AuditLogDto(
                log.Id,
                log.EntityType,
                log.EntityId,
                log.Action,
                log.PerformedBy,
                log.PerformedAt,
                log.Metadata))
            .ToList()
            .AsReadOnly();
    }
}
