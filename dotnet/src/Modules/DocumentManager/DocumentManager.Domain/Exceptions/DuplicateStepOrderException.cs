using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class DuplicateStepOrderException : DomainException
{
    private const string ErrorCode = "INVALID_STEP_ORDER";

    public DuplicateStepOrderException(int stepOrder, Guid workflowId)
        : base(ErrorCode, $"Duplicate step order {stepOrder} found in workflow '{workflowId}'.")
    {
    }
}
