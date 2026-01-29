using Common.Domain;

namespace Identity.Domain.Exceptions;

public sealed class RoleAlreadyExistsException : DomainException
{
    private const string ErrorCode = "ROLE_ALREADY_EXISTS";

    public RoleAlreadyExistsException(string name)
        : base(ErrorCode, $"A role with name '{name}' already exists.")
    {
    }
}
