using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class TagAlreadyAssignedException : DomainException
{
    private const string ErrorCode = "TAG_ALREADY_ASSIGNED";

    public TagAlreadyAssignedException(Guid documentId, Guid tagId)
        : base(ErrorCode, $"Tag '{tagId}' is already assigned to document '{documentId}'.")
    {
    }
}
