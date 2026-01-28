using Common.Domain;

namespace Identity.Domain.Exceptions;

public sealed class InvalidEmailException : DomainException
{
    private const string ErrorCode = "INVALID_EMAIL";

    public InvalidEmailException(string message)
        : base(ErrorCode, message)
    {
    }
}
