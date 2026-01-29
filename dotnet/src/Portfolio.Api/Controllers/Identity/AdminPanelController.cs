using Common.Contracts;
using Identity.Application.UseCases.Admin.Permissions;
using Identity.Application.UseCases.Admin.Roles;
using Identity.Application.UseCases.Admin.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.Identity;

namespace Portfolio.Api.Controllers.Identity;

/// <summary>
/// Admin panel for managing permissions, roles, and user role assignments.
/// </summary>
/// <remarks>
/// This controller provides RBAC (Role-Based Access Control) management capabilities.
/// New users automatically receive the MANAGER role upon registration, which allows them
/// to self-assign additional roles including ADMIN for full system access.
///
/// **Role Hierarchy:**
/// - **ADMIN**: Full system access - bypasses all permission checks
/// - **MANAGER**: Can view roles/permissions and manage their own role assignments
///
/// **Portfolio Privacy Policy:**
/// Since this is a demo application where any user can self-assign the ADMIN role,
/// all users (including admins) can only view and modify their own information.
/// In a production system, admins would have full access to all user data.
/// </remarks>
[ApiController]
[Route("api/admin")]
[Tags("Identity - Admin Panel")]
[Authorize]
[Produces("application/json")]
public class AdminPanelController : ControllerBase
{
    // Permission handlers
    private readonly GetPermissionsHandler _getPermissionsHandler;
    private readonly GetPermissionByIdHandler _getPermissionByIdHandler;
    private readonly CreatePermissionHandler _createPermissionHandler;
    private readonly DeletePermissionHandler _deletePermissionHandler;

    // Role handlers
    private readonly GetRolesHandler _getRolesHandler;
    private readonly GetRoleByIdHandler _getRoleByIdHandler;
    private readonly CreateRoleHandler _createRoleHandler;
    private readonly DeleteRoleHandler _deleteRoleHandler;
    private readonly AssignPermissionToRoleHandler _assignPermissionToRoleHandler;
    private readonly RemovePermissionFromRoleHandler _removePermissionFromRoleHandler;

    // User handlers
    private readonly GetUsersHandler _getUsersHandler;
    private readonly GetUserByIdHandler _getUserByIdHandler;
    private readonly AssignRoleToUserHandler _assignRoleToUserHandler;
    private readonly RemoveRoleFromUserHandler _removeRoleFromUserHandler;
    private readonly GetCurrentUserHandler _getCurrentUserHandler;

    public AdminPanelController(
        GetPermissionsHandler getPermissionsHandler,
        GetPermissionByIdHandler getPermissionByIdHandler,
        CreatePermissionHandler createPermissionHandler,
        DeletePermissionHandler deletePermissionHandler,
        GetRolesHandler getRolesHandler,
        GetRoleByIdHandler getRoleByIdHandler,
        CreateRoleHandler createRoleHandler,
        DeleteRoleHandler deleteRoleHandler,
        AssignPermissionToRoleHandler assignPermissionToRoleHandler,
        RemovePermissionFromRoleHandler removePermissionFromRoleHandler,
        GetUsersHandler getUsersHandler,
        GetUserByIdHandler getUserByIdHandler,
        AssignRoleToUserHandler assignRoleToUserHandler,
        RemoveRoleFromUserHandler removeRoleFromUserHandler,
        GetCurrentUserHandler getCurrentUserHandler)
    {
        _getPermissionsHandler = getPermissionsHandler;
        _getPermissionByIdHandler = getPermissionByIdHandler;
        _createPermissionHandler = createPermissionHandler;
        _deletePermissionHandler = deletePermissionHandler;
        _getRolesHandler = getRolesHandler;
        _getRoleByIdHandler = getRoleByIdHandler;
        _createRoleHandler = createRoleHandler;
        _deleteRoleHandler = deleteRoleHandler;
        _assignPermissionToRoleHandler = assignPermissionToRoleHandler;
        _removePermissionFromRoleHandler = removePermissionFromRoleHandler;
        _getUsersHandler = getUsersHandler;
        _getUserByIdHandler = getUserByIdHandler;
        _assignRoleToUserHandler = assignRoleToUserHandler;
        _removeRoleFromUserHandler = removeRoleFromUserHandler;
        _getCurrentUserHandler = getCurrentUserHandler;
    }

    #region Permissions

    /// <summary>
    /// Retrieves all permissions in the system.
    /// </summary>
    /// <remarks>
    /// Returns a list of all available permissions that can be assigned to roles.
    /// Permissions follow the format `resource:action` (e.g., `document:create`, `admin:manage_roles`).
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all permissions.</returns>
    /// <response code="200">Successfully retrieved permissions.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(ApiResponse<GetPermissionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResponse<GetPermissionsResponse>> GetPermissions(CancellationToken cancellationToken)
    {
        var result = await _getPermissionsHandler.HandleAsync(cancellationToken);
        var response = new GetPermissionsResponse(
            result.Permissions.Select(p => new PermissionResponse(p.Id, p.Name, p.Description, p.CreatedAt)).ToList());
        return ApiResponse.Success(response);
    }

    /// <summary>
    /// Retrieves a specific permission by ID.
    /// </summary>
    /// <param name="id">The permission ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The permission details.</returns>
    /// <response code="200">Successfully retrieved permission.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Permission not found.</response>
    [HttpGet("permissions/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<PermissionResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<PermissionResponse>> GetPermissionById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPermissionByIdQuery(id);
        var result = await _getPermissionByIdHandler.HandleAsync(query, cancellationToken);
        return ApiResponse.Success(new PermissionResponse(result.Id, result.Name, result.Description, result.CreatedAt));
    }

    /// <summary>
    /// Creates a new permission.
    /// </summary>
    /// <remarks>
    /// **Requires ADMIN role.** Permission names are automatically converted to uppercase.
    /// Use the format `resource:action` for consistency (e.g., `report:generate`).
    /// </remarks>
    /// <param name="request">The permission creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created permission.</returns>
    /// <response code="201">Permission successfully created.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="409">A permission with this name already exists.</response>
    [HttpPost("permissions")]
    [ProducesResponseType(typeof(ApiResponse<CreatePermissionResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CreatePermissionResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<CreatePermissionResponse>), StatusCodes.Status409Conflict)]
    public async Task<ApiResponse<CreatePermissionResponse>> CreatePermission(
        [FromBody] CreatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePermissionCommand(request.Name, request.Description);
        var result = await _createPermissionHandler.HandleAsync(command, cancellationToken);
        return ApiResponse.Created(new CreatePermissionResponse(result.Id, result.Name, result.Description));
    }

    /// <summary>
    /// Deletes a permission.
    /// </summary>
    /// <remarks>
    /// **Requires ADMIN role.** This will remove the permission from all roles that have it assigned.
    /// Use with caution as this may affect user access.
    /// </remarks>
    /// <param name="id">The permission ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Permission successfully deleted.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Permission not found.</response>
    [HttpDelete("permissions/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<object>> DeletePermission(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePermissionCommand(id);
        await _deletePermissionHandler.HandleAsync(command, cancellationToken);
        return ApiResponse.NoContent();
    }

    #endregion

    #region Roles

    /// <summary>
    /// Retrieves all roles with their assigned permissions.
    /// </summary>
    /// <remarks>
    /// Returns all roles in the system. Use this to discover available roles
    /// and their permission sets before assigning roles to users.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all roles with their permissions.</returns>
    /// <response code="200">Successfully retrieved roles.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(ApiResponse<GetRolesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResponse<GetRolesResponse>> GetRoles(CancellationToken cancellationToken)
    {
        var result = await _getRolesHandler.HandleAsync(cancellationToken);
        var response = new GetRolesResponse(
            result.Roles.Select(r => new RoleResponse(r.Id, r.Name, r.Description, r.CreatedAt, r.Permissions)).ToList());
        return ApiResponse.Success(response);
    }

    /// <summary>
    /// Retrieves a specific role by ID with its permissions.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The role details including assigned permissions.</returns>
    /// <response code="200">Successfully retrieved role.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Role not found.</response>
    [HttpGet("roles/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<RoleResponse>> GetRoleById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await _getRoleByIdHandler.HandleAsync(query, cancellationToken);
        return ApiResponse.Success(new RoleResponse(result.Id, result.Name, result.Description, result.CreatedAt, result.Permissions));
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    /// <remarks>
    /// **Requires ADMIN role.** Role names are automatically converted to uppercase.
    /// After creation, use the assign permission endpoint to add permissions to the role.
    /// </remarks>
    /// <param name="request">The role creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created role.</returns>
    /// <response code="201">Role successfully created.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="409">A role with this name already exists.</response>
    [HttpPost("roles")]
    [ProducesResponseType(typeof(ApiResponse<CreateRoleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CreateRoleResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<CreateRoleResponse>), StatusCodes.Status409Conflict)]
    public async Task<ApiResponse<CreateRoleResponse>> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoleCommand(request.Name, request.Description);
        var result = await _createRoleHandler.HandleAsync(command, cancellationToken);
        return ApiResponse.Created(new CreateRoleResponse(result.Id, result.Name, result.Description));
    }

    /// <summary>
    /// Deletes a role.
    /// </summary>
    /// <remarks>
    /// **Requires ADMIN role.** This will remove the role from all users that have it assigned.
    /// Use with caution as this may affect user access.
    /// </remarks>
    /// <param name="id">The role ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Role successfully deleted.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Role not found.</response>
    [HttpDelete("roles/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<object>> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteRoleCommand(id);
        await _deleteRoleHandler.HandleAsync(command, cancellationToken);
        return ApiResponse.NoContent();
    }

    /// <summary>
    /// Assigns a permission to a role.
    /// </summary>
    /// <remarks>
    /// **Requires ADMIN role.** Users with this role will immediately gain the permission
    /// on their next authentication (token refresh).
    /// </remarks>
    /// <param name="roleId">The role ID.</param>
    /// <param name="permissionId">The permission ID to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated role with its permissions.</returns>
    /// <response code="200">Permission successfully assigned to role.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Role or permission not found.</response>
    [HttpPost("roles/{roleId:guid}/permissions/{permissionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<RoleResponse>> AssignPermissionToRole(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var command = new AssignPermissionToRoleCommand(roleId, permissionId);
        var result = await _assignPermissionToRoleHandler.HandleAsync(command, cancellationToken);
        return ApiResponse.Success(new RoleResponse(
            result.Id, result.Name, result.Description, result.CreatedAt, result.Permissions));
    }

    /// <summary>
    /// Removes a permission from a role.
    /// </summary>
    /// <remarks>
    /// **Requires ADMIN role.** Users with this role will lose the permission
    /// on their next authentication (token refresh).
    /// </remarks>
    /// <param name="roleId">The role ID.</param>
    /// <param name="permissionId">The permission ID to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated role with its permissions.</returns>
    /// <response code="200">Permission successfully removed from role.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Role or permission not found.</response>
    [HttpDelete("roles/{roleId:guid}/permissions/{permissionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<RoleResponse>> RemovePermissionFromRole(
        Guid roleId,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        var command = new RemovePermissionFromRoleCommand(roleId, permissionId);
        var result = await _removePermissionFromRoleHandler.HandleAsync(command, cancellationToken);
        return ApiResponse.Success(new RoleResponse(
            result.Id, result.Name, result.Description, result.CreatedAt, result.Permissions));
    }

    #endregion

    #region Users

    /// <summary>
    /// Retrieves users list (portfolio: returns only current user).
    /// </summary>
    /// <remarks>
    /// **Portfolio Restriction:** Since users can self-assign the ADMIN role in this demo,
    /// this endpoint only returns the current authenticated user to protect privacy.
    /// In a production system, this would return all users for administrators.
    ///
    /// Use `GET /api/admin/me` for a more semantic way to retrieve your own details.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List containing only the current user.</returns>
    /// <response code="200">Successfully retrieved user information.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<GetUsersResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResponse<GetUsersResponse>> GetUsers(CancellationToken cancellationToken)
    {
        var result = await _getUsersHandler.HandleAsync(cancellationToken);
        var response = new GetUsersResponse(
            result.Users.Select(u => new UserResponse(
                u.Id, u.Email, u.FirstName, u.LastName, u.FullName, u.Status.ToString(), u.CreatedAt, u.LastLoginAt, u.Roles)).ToList());
        return ApiResponse.Success(response);
    }

    /// <summary>
    /// Retrieves a user by ID.
    /// </summary>
    /// <remarks>
    /// **Portfolio Restriction:** Users can only retrieve their own information.
    /// In a production system, admins would be able to view any user's data.
    /// Includes detailed information such as assigned roles and effective permissions.
    /// </remarks>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user details including roles and permissions.</returns>
    /// <response code="200">Successfully retrieved user.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to view this user.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("users/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<UserDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<UserDetailResponse>> GetUserById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await _getUserByIdHandler.HandleAsync(query, cancellationToken);
        return ApiResponse.Success(new UserDetailResponse(
            result.Id, result.Email, result.FirstName, result.LastName, result.FullName,
            result.Status.ToString(), result.CreatedAt, result.LastLoginAt, result.Roles, result.Permissions));
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <remarks>
    /// **Portfolio Restriction:** Users can only assign roles to themselves.
    /// In a production system, admins would be able to manage any user's roles.
    ///
    /// **Demo Feature:** Users can self-assign the ADMIN role to explore all system features.
    /// After assignment, re-login to get an updated token.
    ///
    /// **Tip:** Use `GET /api/admin/roles` to find available role IDs.
    /// </remarks>
    /// <param name="userId">The user ID to assign the role to.</param>
    /// <param name="roleId">The role ID to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Role successfully assigned to user.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to manage this user's roles.</response>
    /// <response code="404">User or role not found.</response>
    [HttpPost("users/{userId:guid}/roles/{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<object>> AssignRoleToUser(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var command = new AssignRoleToUserCommand(userId, roleId);
        await _assignRoleToUserHandler.HandleAsync(command, cancellationToken);
        return ApiResponse.NoContent();
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    /// <remarks>
    /// **Portfolio Restriction:** Users can only remove roles from themselves.
    /// In a production system, admins would be able to manage any user's roles.
    /// After removal, re-login to get an updated token reflecting the change.
    /// </remarks>
    /// <param name="userId">The user ID to remove the role from.</param>
    /// <param name="roleId">The role ID to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Role successfully removed from user.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to manage this user's roles.</response>
    /// <response code="404">User or role not found.</response>
    [HttpDelete("users/{userId:guid}/roles/{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<object>> RemoveRoleFromUser(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveRoleFromUserCommand(userId, roleId);
        await _removeRoleFromUserHandler.HandleAsync(command, cancellationToken);
        return ApiResponse.NoContent();
    }

    /// <summary>
    /// Retrieves the current authenticated user's information.
    /// </summary>
    /// <remarks>
    /// Returns complete details about the authenticated user including their ID,
    /// profile information, assigned roles, and effective permissions.
    /// Use the user ID from this response for self-management operations.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current user's details.</returns>
    /// <response code="200">Successfully retrieved current user information.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResponse<UserDetailResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _getCurrentUserHandler.HandleAsync(cancellationToken);
        return ApiResponse.Success(new UserDetailResponse(
            result.Id, result.Email, result.FirstName, result.LastName, result.FullName,
            result.Status.ToString(), result.CreatedAt, result.LastLoginAt, result.Roles, result.Permissions));
    }

    #endregion
}
