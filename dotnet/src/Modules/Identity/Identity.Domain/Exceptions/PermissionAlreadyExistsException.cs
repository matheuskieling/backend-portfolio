using Common.Domain;

namespace Identity.Domain.Exceptions;

public sealed class PermissionAlreadyExistsException : DomainException
{
    private const string ErrorCode = "PERMISSION_ALREADY_EXISTS";

    public PermissionAlreadyExistsException(string name)
        : base(ErrorCode, $"A permission with name '{name}' already exists.")
    {
    }
}
