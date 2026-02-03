using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class DuplicateStepOrderException : ConflictException
{
    private const string ErrorCode = "INVALID_STEP_ORDER";

    public DuplicateStepOrderException(int stepOrder, Guid workflowId)
        : base(ErrorCode, $"Duplicate step order {stepOrder} found in workflow '{workflowId}'.")
    {
    }
}
