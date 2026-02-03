using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class UnauthorizedDocumentAccessException : ForbiddenException
{
    private const string ErrorCode = "UNAUTHORIZED_DOCUMENT_ACCESS";

    public UnauthorizedDocumentAccessException(Guid documentId, Guid userId)
        : base(ErrorCode, $"User '{userId}' is not authorized to access document '{documentId}'.")
    {
    }

    public UnauthorizedDocumentAccessException(Guid documentId, string reason)
        : base(ErrorCode, $"Access to document '{documentId}' is denied: {reason}")
    {
    }
}
