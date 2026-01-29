using Identity.Application.Repositories;

namespace Identity.Application.UseCases.Admin.Roles;

public sealed record GetRolesQuery;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    IReadOnlyList<string> Permissions);

public sealed record GetRolesResult(IReadOnlyList<RoleDto> Roles);

public sealed class GetRolesHandler
{
    private readonly IRoleRepository _roleRepository;

    public GetRolesHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<GetRolesResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);

        var dtos = roles
            .Select(r => new RoleDto(
                r.Id,
                r.Name,
                r.Description,
                r.CreatedAt,
                r.GetPermissionNames().ToList()))
            .ToList();

        return new GetRolesResult(dtos);
    }
}
