using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class InvalidStoragePathException : DomainException
{
    private const string ErrorCode = "INVALID_STORAGE_PATH";

    public InvalidStoragePathException(string message)
        : base(ErrorCode, message)
    {
    }
}
