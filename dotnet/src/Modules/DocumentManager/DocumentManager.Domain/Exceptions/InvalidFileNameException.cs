using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class InvalidFileNameException : DomainException
{
    private const string ErrorCode = "INVALID_FILE_NAME";

    public InvalidFileNameException(string message)
        : base(ErrorCode, message)
    {
    }
}
