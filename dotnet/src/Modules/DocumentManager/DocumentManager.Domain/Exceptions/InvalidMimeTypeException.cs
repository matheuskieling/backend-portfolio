using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class InvalidMimeTypeException : DomainException
{
    private const string ErrorCode = "INVALID_MIME_TYPE";

    public InvalidMimeTypeException(string message)
        : base(ErrorCode, message)
    {
    }
}
