using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class WorkflowNotActiveException : DomainException
{
    private const string ErrorCode = "WORKFLOW_NOT_ACTIVE";

    public WorkflowNotActiveException(Guid workflowId)
        : base(ErrorCode, $"Workflow with ID '{workflowId}' is not active and cannot be used.")
    {
    }
}
