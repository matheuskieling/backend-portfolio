using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class DuplicateStepOrderException : DomainException
{
    private const string ErrorCode = "DUPLICATE_STEP_ORDER";

    public DuplicateStepOrderException(int stepOrder, Guid workflowId)
        : base(ErrorCode, $"A step with order {stepOrder} already exists in workflow '{workflowId}'.")
    {
    }
}
