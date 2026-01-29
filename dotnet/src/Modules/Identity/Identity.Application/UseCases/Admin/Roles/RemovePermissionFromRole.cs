using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Roles;

public sealed record RemovePermissionFromRoleCommand(Guid RoleId, Guid PermissionId);

public sealed record RemovePermissionFromRoleResult(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    IReadOnlyList<string> Permissions);

public sealed class RemovePermissionFromRoleHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public RemovePermissionFromRoleHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RemovePermissionFromRoleResult> HandleAsync(
        RemovePermissionFromRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(command.RoleId, cancellationToken)
            ?? throw new RoleNotFoundException(command.RoleId);

        var permission = await _permissionRepository.GetByIdAsync(command.PermissionId, cancellationToken)
            ?? throw new PermissionNotFoundException(command.PermissionId);

        role.RemovePermission(permission);

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RemovePermissionFromRoleResult(
            role.Id,
            role.Name,
            role.Description,
            role.CreatedAt,
            role.GetPermissionNames().ToList());
    }
}
