using Common.Domain;

namespace Identity.Domain.Entities;

public sealed class Permission : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Permission() : base() { }

    private Permission(string name, string? description) : base()
    {
        Name = name;
        Description = description;
    }

    public static Permission Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Permission name cannot be empty.", nameof(name));

        return new Permission(name.Trim().ToLowerInvariant(), description?.Trim());
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        SetUpdated();
    }
}
