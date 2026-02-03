using Common.Domain.Exceptions;

namespace Identity.Domain.Exceptions;

public sealed class UserAlreadyExistsException : ConflictException
{
    private const string ErrorCode = "USER_ALREADY_EXISTS";

    public UserAlreadyExistsException(string email)
        : base(ErrorCode, $"A user with email '{email}' already exists.")
    {
    }
}
