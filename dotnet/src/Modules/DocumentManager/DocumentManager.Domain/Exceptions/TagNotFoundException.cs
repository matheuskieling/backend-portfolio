using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class TagNotFoundException : NotFoundException
{
    private const string ErrorCode = "TAG_NOT_FOUND";

    public TagNotFoundException(Guid tagId)
        : base(ErrorCode, $"Tag with ID '{tagId}' was not found.")
    {
    }

    public TagNotFoundException(string tagName)
        : base(ErrorCode, $"Tag with name '{tagName}' was not found.")
    {
    }
}
