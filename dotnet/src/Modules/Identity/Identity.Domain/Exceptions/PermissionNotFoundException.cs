using Common.Domain.Exceptions;

namespace Identity.Domain.Exceptions;

public sealed class PermissionNotFoundException : NotFoundException
{
    private const string ErrorCode = "PERMISSION_NOT_FOUND";

    public PermissionNotFoundException(Guid permissionId)
        : base(ErrorCode, $"Permission with ID '{permissionId}' was not found.")
    {
    }

    public PermissionNotFoundException(string permissionName)
        : base(ErrorCode, $"Permission '{permissionName}' was not found.")
    {
    }
}
