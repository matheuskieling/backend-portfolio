using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class TagNotAssignedException : ValidationException
{
    private const string ErrorCode = "TAG_NOT_ASSIGNED";

    public TagNotAssignedException(Guid documentId, Guid tagId)
        : base(ErrorCode, $"Tag '{tagId}' is not assigned to document '{documentId}'.")
    {
    }
}
