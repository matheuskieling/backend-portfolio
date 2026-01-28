using Common.Domain;
using Identity.Domain.Enums;
using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Entities;

public sealed class User : AuditableEntity, IAggregateRoot
{
    public Email Email { get; private set; } = null!;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public UserStatus Status { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEndAt { get; private set; }

    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private User() : base() { }

    private User(Email email, PasswordHash passwordHash, string firstName, string lastName)
        : base()
    {
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
    }

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        var emailVo = Email.Create(email);
        var passwordHashVo = PasswordHash.Create(passwordHash);

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        return new User(emailVo, passwordHashVo, firstName.Trim(), lastName.Trim());
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = PasswordHash.Create(newPasswordHash);
        SetUpdated();
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        SetUpdated();
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        LockoutEndAt = null;
        SetUpdated();
    }

    public void Deactivate()
    {
        Status = UserStatus.Deactivated;
        SetUpdated();
    }

    public void Lock(TimeSpan duration)
    {
        Status = UserStatus.Locked;
        LockoutEndAt = DateTime.UtcNow.Add(duration);
        SetUpdated();
    }

    public void Unlock()
    {
        if (Status == UserStatus.Locked)
        {
            Status = UserStatus.Active;
            LockoutEndAt = null;
            FailedLoginAttempts = 0;
            SetUpdated();
        }
    }

    public bool IsLocked()
    {
        if (Status != UserStatus.Locked)
            return false;

        if (LockoutEndAt.HasValue && LockoutEndAt.Value <= DateTime.UtcNow)
        {
            Unlock();
            return false;
        }

        return true;
    }

    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        SetUpdated();
    }

    public void RecordFailedLogin(int maxAttempts = 5, TimeSpan? lockoutDuration = null)
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= maxAttempts)
        {
            Lock(lockoutDuration ?? TimeSpan.FromMinutes(15));
        }

        SetUpdated();
    }

    public void AssignRole(Role role)
    {
        if (_userRoles.Any(ur => ur.RoleId == role.Id))
            return;

        var userRole = UserRole.Create(this, role);
        _userRoles.Add(userRole);
        SetUpdated();
    }

    public void RemoveRole(Role role)
    {
        var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
        if (userRole is not null)
        {
            _userRoles.Remove(userRole);
            SetUpdated();
        }
    }

    public bool HasRole(string roleName)
    {
        return _userRoles.Any(ur =>
            string.Equals(ur.Role.Name, roleName, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasPermission(string permissionName)
    {
        return _userRoles.Any(ur => ur.Role.HasPermission(permissionName));
    }

    public IEnumerable<string> GetRoleNames()
    {
        return _userRoles.Select(ur => ur.Role.Name);
    }

    public IEnumerable<string> GetPermissions()
    {
        return _userRoles
            .SelectMany(ur => ur.Role.GetPermissionNames())
            .Distinct();
    }

    public void EnsureCanLogin()
    {
        if (Status == UserStatus.Deactivated)
            throw new UserDeactivatedException(Id);

        if (IsLocked())
            throw new UserDeactivatedException(Id);

        if (Status == UserStatus.PendingVerification)
            throw new InvalidOperationException("User email is not verified.");
    }
}
