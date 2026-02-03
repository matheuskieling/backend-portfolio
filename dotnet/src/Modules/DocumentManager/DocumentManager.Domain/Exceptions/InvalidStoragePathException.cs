using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class InvalidStoragePathException : ValidationException
{
    private const string ErrorCode = "INVALID_STORAGE_PATH";

    public InvalidStoragePathException(string message)
        : base(ErrorCode, message)
    {
    }
}
