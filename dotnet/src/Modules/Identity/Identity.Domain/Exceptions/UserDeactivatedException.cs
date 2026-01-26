using Identity.Domain.Common;

namespace Identity.Domain.Exceptions;

public sealed class UserDeactivatedException : DomainException
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
