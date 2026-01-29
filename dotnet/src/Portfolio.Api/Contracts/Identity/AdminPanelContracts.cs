namespace Portfolio.Api.Contracts.Identity;

#region Permissions

/// <summary>
/// Request to create a new permission.
/// </summary>
/// <param name="Name">The permission name (e.g., "document:create"). Will be converted to uppercase.</param>
/// <param name="Description">Optional description of what this permission allows.</param>
public sealed record CreatePermissionRequest(string Name, string? Description);

/// <summary>
/// Response after creating a permission.
/// </summary>
public sealed record CreatePermissionResponse(Guid Id, string Name, string? Description);

/// <summary>
/// Permission details.
/// </summary>
public sealed record PermissionResponse(Guid Id, string Name, string? Description, DateTime CreatedAt);

/// <summary>
/// List of permissions.
/// </summary>
public sealed record GetPermissionsResponse(IReadOnlyList<PermissionResponse> Permissions);

#endregion

#region Roles

/// <summary>
/// Request to create a new role.
/// </summary>
/// <param name="Name">The role name (e.g., "EDITOR"). Will be converted to uppercase.</param>
/// <param name="Description">Optional description of this role's purpose.</param>
public sealed record CreateRoleRequest(string Name, string? Description);

/// <summary>
/// Response after creating a role.
/// </summary>
public sealed record CreateRoleResponse(Guid Id, string Name, string? Description);

/// <summary>
/// Role details including assigned permissions.
/// </summary>
public sealed record RoleResponse(Guid Id, string Name, string? Description, DateTime CreatedAt, IReadOnlyList<string> Permissions);

/// <summary>
/// List of roles.
/// </summary>
public sealed record GetRolesResponse(IReadOnlyList<RoleResponse> Roles);

#endregion

#region Users

/// <summary>
/// Basic user information.
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles);

/// <summary>
/// Detailed user information including effective permissions.
/// </summary>
public sealed record UserDetailResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

/// <summary>
/// List of users.
/// </summary>
public sealed record GetUsersResponse(IReadOnlyList<UserResponse> Users);

#endregion
