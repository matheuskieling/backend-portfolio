using Common.Domain;

namespace DocumentManager.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = null!;
    public Guid? PerformedBy { get; private set; }
    public DateTime PerformedAt { get; private set; }
    public string? Metadata { get; private set; }

    private AuditLog() : base() { }

    private AuditLog(
        string entityType,
        Guid entityId,
        string action,
        Guid? performedBy,
        string? metadata) : base()
    {
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        PerformedBy = performedBy;
        PerformedAt = DateTime.UtcNow;
        Metadata = metadata;
    }

    public static AuditLog Create(
        string entityType,
        Guid entityId,
        string action,
        Guid? performedBy,
        string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty.", nameof(entityType));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty.", nameof(action));

        return new AuditLog(entityType, entityId, action, performedBy, metadata);
    }
}
