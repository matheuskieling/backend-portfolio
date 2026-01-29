using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Permissions;

public sealed record DeletePermissionCommand(Guid Id);

public sealed class DeletePermissionHandler
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public DeletePermissionHandler(
        IPermissionRepository permissionRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        DeletePermissionCommand command,
        CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new PermissionNotFoundException(command.Id);

        _permissionRepository.Remove(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
