using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Entities;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Permissions;

public sealed record CreatePermissionCommand(string Name, string? Description);

public sealed record CreatePermissionResult(Guid Id, string Name, string? Description);

public sealed class CreatePermissionHandler
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public CreatePermissionHandler(
        IPermissionRepository permissionRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreatePermissionResult> HandleAsync(
        CreatePermissionCommand command,
        CancellationToken cancellationToken = default)
    {
        var exists = await _permissionRepository.ExistsByNameAsync(command.Name, cancellationToken);
        if (exists)
        {
            throw new PermissionAlreadyExistsException(command.Name);
        }

        var permission = Permission.Create(command.Name, command.Description);

        _permissionRepository.Add(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreatePermissionResult(permission.Id, permission.Name, permission.Description);
    }
}
