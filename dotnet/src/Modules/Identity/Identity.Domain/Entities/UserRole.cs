using Common.Domain;

namespace Identity.Domain.Entities;

public sealed class UserRole : BaseEntity
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public Guid RoleId { get; private set; }
    public Role Role { get; private set; } = null!;

    public DateTime AssignedAt { get; private set; }

    private UserRole() : base() { }

    internal static UserRole Create(User user, Role role)
    {
        return new UserRole
        {
            UserId = user.Id,
            User = user,
            RoleId = role.Id,
            Role = role,
            AssignedAt = DateTime.UtcNow
        };
    }
}
