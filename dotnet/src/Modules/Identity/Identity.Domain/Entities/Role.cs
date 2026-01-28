using Common.Domain;

namespace Identity.Domain.Entities;

public sealed class Role : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private readonly List<RolePermission> _rolePermissions = new();
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role() : base() { }

    private Role(string name, string? description) : base()
    {
        Name = name;
        Description = description;
    }

    public static Role Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name cannot be empty.", nameof(name));

        return new Role(name.Trim().ToUpperInvariant(), description?.Trim());
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
        SetUpdated();
    }

    public void AssignPermission(Permission permission)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permission.Id))
            return;

        var rolePermission = RolePermission.Create(this, permission);
        _rolePermissions.Add(rolePermission);
        SetUpdated();
    }

    public void RemovePermission(Permission permission)
    {
        var rolePermission = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permission.Id);
        if (rolePermission is not null)
        {
            _rolePermissions.Remove(rolePermission);
            SetUpdated();
        }
    }

    public bool HasPermission(string permissionName)
    {
        return _rolePermissions.Any(rp =>
            string.Equals(rp.Permission.Name, permissionName, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<string> GetPermissionNames()
    {
        return _rolePermissions.Select(rp => rp.Permission.Name);
    }
}
