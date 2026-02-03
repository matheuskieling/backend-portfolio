using Common.Domain.Exceptions;

namespace Identity.Domain.Exceptions;

public sealed class RoleAlreadyExistsException : ConflictException
{
    private const string ErrorCode = "ROLE_ALREADY_EXISTS";

    public RoleAlreadyExistsException(string name)
        : base(ErrorCode, $"A role with name '{name}' already exists.")
    {
    }
}
