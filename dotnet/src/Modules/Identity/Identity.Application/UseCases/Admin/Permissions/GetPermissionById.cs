using Identity.Application.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Permissions;

public sealed record GetPermissionByIdQuery(Guid Id);

public sealed record GetPermissionByIdResult(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt);

public sealed class GetPermissionByIdHandler
{
    private readonly IPermissionRepository _permissionRepository;

    public GetPermissionByIdHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<GetPermissionByIdResult> HandleAsync(
        GetPermissionByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var permission = await _permissionRepository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new PermissionNotFoundException(query.Id);

        return new GetPermissionByIdResult(
            permission.Id,
            permission.Name,
            permission.Description,
            permission.CreatedAt);
    }
}
