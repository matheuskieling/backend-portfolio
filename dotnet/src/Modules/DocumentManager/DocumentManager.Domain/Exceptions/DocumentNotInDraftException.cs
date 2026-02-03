using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class DocumentNotInDraftException : ValidationException
{
    private const string ErrorCode = "DOCUMENT_NOT_IN_DRAFT";

    public DocumentNotInDraftException(Guid documentId)
        : base(ErrorCode, $"Document with ID '{documentId}' is not in Draft status and cannot be modified.")
    {
    }
}
