using Common.Domain.Exceptions;

namespace Identity.Domain.Exceptions;

public sealed class InvalidEmailException : ValidationException
{
    private const string ErrorCode = "INVALID_EMAIL";

    public InvalidEmailException(string message)
        : base(ErrorCode, message)
    {
    }
}
