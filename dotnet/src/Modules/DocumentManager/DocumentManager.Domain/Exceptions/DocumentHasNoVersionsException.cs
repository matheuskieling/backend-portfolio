using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class DocumentHasNoVersionsException : DomainException
{
    private const string ErrorCode = "DOCUMENT_HAS_NO_VERSIONS";

    public DocumentHasNoVersionsException(Guid documentId)
        : base(ErrorCode, $"Document with ID '{documentId}' has no versions and cannot be submitted for approval.")
    {
    }
}
