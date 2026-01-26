using Identity.Domain.Common;

namespace Identity.Domain.Exceptions;

public sealed class UserNotFoundException : DomainException
{
    private const string ErrorCode = "USER_NOT_FOUND";

    public UserNotFoundException(Guid userId)
        : base(ErrorCode, $"User with ID '{userId}' was not found.")
    {
    }

    public UserNotFoundException(string email)
        : base(ErrorCode, $"User with email '{email}' was not found.")
    {
    }
}
