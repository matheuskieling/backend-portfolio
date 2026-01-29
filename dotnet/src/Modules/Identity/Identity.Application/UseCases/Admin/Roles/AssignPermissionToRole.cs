using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Roles;

public sealed record AssignPermissionToRoleCommand(Guid RoleId, Guid PermissionId);

public sealed record AssignPermissionToRoleResult(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    IReadOnlyList<string> Permissions);

public sealed class AssignPermissionToRoleHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public AssignPermissionToRoleHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssignPermissionToRoleResult> HandleAsync(
        AssignPermissionToRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(command.RoleId, cancellationToken)
            ?? throw new RoleNotFoundException(command.RoleId);

        var permission = await _permissionRepository.GetByIdAsync(command.PermissionId, cancellationToken)
            ?? throw new PermissionNotFoundException(command.PermissionId);

        role.AssignPermission(permission);

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AssignPermissionToRoleResult(
            role.Id,
            role.Name,
            role.Description,
            role.CreatedAt,
            role.GetPermissionNames().ToList());
    }
}
