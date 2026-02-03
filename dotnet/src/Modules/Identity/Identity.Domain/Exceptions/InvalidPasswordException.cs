using Common.Domain.Exceptions;

namespace Identity.Domain.Exceptions;

public sealed class InvalidPasswordException : ValidationException
{
    private const string ErrorCode = "INVALID_PASSWORD";

    public InvalidPasswordException(string message)
        : base(ErrorCode, message)
    {
    }
}
