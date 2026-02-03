using Common.Domain.Exceptions;

namespace Identity.Domain.Exceptions;

public sealed class UserDeactivatedException : ValidationException
{
    private const string ErrorCode = "USER_DEACTIVATED";

    public UserDeactivatedException(Guid userId)
        : base(ErrorCode, $"User with ID '{userId}' is deactivated.")
    {
    }

    public UserDeactivatedException(string email)
        : base(ErrorCode, $"User with email '{email}' is deactivated.")
    {
    }
}
