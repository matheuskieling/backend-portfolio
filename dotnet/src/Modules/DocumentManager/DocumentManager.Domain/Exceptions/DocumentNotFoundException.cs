using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class DocumentNotFoundException : NotFoundException
{
    private const string ErrorCode = "DOCUMENT_NOT_FOUND";

    public DocumentNotFoundException(Guid documentId)
        : base(ErrorCode, $"Document with ID '{documentId}' was not found.")
    {
    }
}
