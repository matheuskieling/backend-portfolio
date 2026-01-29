using Identity.Application.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Roles;

public sealed record GetRoleByIdQuery(Guid Id);

public sealed record GetRoleByIdResult(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    IReadOnlyList<string> Permissions);

public sealed class GetRoleByIdHandler
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByIdHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<GetRoleByIdResult> HandleAsync(
        GetRoleByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(query.Id, cancellationToken)
            ?? throw new RoleNotFoundException(query.Id);

        return new GetRoleByIdResult(
            role.Id,
            role.Name,
            role.Description,
            role.CreatedAt,
            role.GetPermissionNames().ToList());
    }
}
