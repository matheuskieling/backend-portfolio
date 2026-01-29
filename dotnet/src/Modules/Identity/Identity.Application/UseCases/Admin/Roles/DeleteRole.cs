using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Roles;

public sealed record DeleteRoleCommand(Guid Id);

public sealed class DeleteRoleHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public DeleteRoleHandler(
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        DeleteRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new RoleNotFoundException(command.Id);

        _roleRepository.Remove(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
