namespace DocumentManager.Application.DTOs;

public sealed record AuditLogDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    Guid? PerformedBy,
    DateTime PerformedAt,
    string? Metadata);
