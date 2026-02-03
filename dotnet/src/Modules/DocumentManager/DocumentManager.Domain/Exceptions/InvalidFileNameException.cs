using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class InvalidFileNameException : ValidationException
{
    private const string ErrorCode = "INVALID_FILE_NAME";

    public InvalidFileNameException(string message)
        : base(ErrorCode, message)
    {
    }
}
