using Common.Domain.Exceptions;

namespace Identity.Domain.Exceptions;

public sealed class RoleNotFoundException : NotFoundException
{
    private const string ErrorCode = "ROLE_NOT_FOUND";

    public RoleNotFoundException(Guid roleId)
        : base(ErrorCode, $"Role with ID '{roleId}' was not found.")
    {
    }

    public RoleNotFoundException(string roleName)
        : base(ErrorCode, $"Role '{roleName}' was not found.")
    {
    }
}
