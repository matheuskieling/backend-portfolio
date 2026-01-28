using Common.Domain;
using DocumentManager.Domain.Enums;

namespace DocumentManager.Domain.Exceptions;

public sealed class InvalidDocumentStateException : DomainException
{
    private const string ErrorCode = "INVALID_DOCUMENT_STATE";

    public InvalidDocumentStateException(Guid documentId, DocumentStatus currentStatus, DocumentStatus requiredStatus)
        : base(ErrorCode, $"Document with ID '{documentId}' is in '{currentStatus}' status but must be in '{requiredStatus}' status for this operation.")
    {
    }

    public InvalidDocumentStateException(Guid documentId, string message)
        : base(ErrorCode, $"Document with ID '{documentId}': {message}")
    {
    }
}
