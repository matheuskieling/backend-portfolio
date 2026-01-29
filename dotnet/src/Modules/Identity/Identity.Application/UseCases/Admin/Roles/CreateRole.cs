using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Entities;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Roles;

public sealed record CreateRoleCommand(string Name, string? Description);

public sealed record CreateRoleResult(Guid Id, string Name, string? Description);

public sealed class CreateRoleHandler
{
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public CreateRoleHandler(
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateRoleResult> HandleAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        var exists = await _roleRepository.ExistsByNameAsync(command.Name, cancellationToken);
        if (exists)
        {
            throw new RoleAlreadyExistsException(command.Name);
        }

        var role = Role.Create(command.Name, command.Description);

        _roleRepository.Add(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateRoleResult(role.Id, role.Name, role.Description);
    }
}
