using Identity.Domain.Common;

namespace Identity.Domain.Exceptions;

public sealed class InvalidPasswordException : DomainException
{
    private const string ErrorCode = "INVALID_PASSWORD";

    public InvalidPasswordException(string message)
        : base(ErrorCode, message)
    {
    }
}
