using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class InvalidMimeTypeException : ValidationException
{
    private const string ErrorCode = "INVALID_MIME_TYPE";

    public InvalidMimeTypeException(string message)
        : base(ErrorCode, message)
    {
    }
}
