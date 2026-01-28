using Common.Domain;

namespace Identity.Domain.Exceptions;

public sealed class InvalidPasswordException : DomainException
{
    private const string ErrorCode = "INVALID_PASSWORD";

    public InvalidPasswordException(string message)
        : base(ErrorCode, message)
    {
    }
}
