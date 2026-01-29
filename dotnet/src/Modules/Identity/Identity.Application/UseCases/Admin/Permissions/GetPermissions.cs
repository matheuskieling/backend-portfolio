using Identity.Application.Repositories;

namespace Identity.Application.UseCases.Admin.Permissions;

public sealed record GetPermissionsQuery;

public sealed record PermissionDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt);

public sealed record GetPermissionsResult(IReadOnlyList<PermissionDto> Permissions);

public sealed class GetPermissionsHandler
{
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionsHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<GetPermissionsResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetAllAsync(cancellationToken);

        var dtos = permissions
            .Select(p => new PermissionDto(p.Id, p.Name, p.Description, p.CreatedAt))
            .ToList();

        return new GetPermissionsResult(dtos);
    }
}
