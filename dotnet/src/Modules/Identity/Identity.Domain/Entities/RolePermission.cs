using Identity.Domain.Common;

namespace Identity.Domain.Entities;

public sealed class RolePermission : BaseEntity
{
    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;

    public Guid PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;

    public DateTime AssignedAt { get; private set; }

    private RolePermission() : base() { }

    internal static RolePermission Create(Role role, Permission permission)
    {
        return new RolePermission
        {
            RoleId = role.Id,
            Role = role,
            PermissionId = permission.Id,
            Permission = permission,
            AssignedAt = DateTime.UtcNow
        };
    }
}
