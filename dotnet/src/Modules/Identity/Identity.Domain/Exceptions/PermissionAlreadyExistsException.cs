using Common.Domain.Exceptions;

namespace Identity.Domain.Exceptions;

public sealed class PermissionAlreadyExistsException : ConflictException
{
    private const string ErrorCode = "PERMISSION_ALREADY_EXISTS";

    public PermissionAlreadyExistsException(string name)
        : base(ErrorCode, $"A permission with name '{name}' already exists.")
    {
    }
}
