using Common.Domain;

namespace DocumentManager.Domain.Exceptions;

public sealed class WorkflowNotFoundException : DomainException
{
    private const string ErrorCode = "WORKFLOW_NOT_FOUND";

    public WorkflowNotFoundException(Guid workflowId)
        : base(ErrorCode, $"Workflow with ID '{workflowId}' was not found.")
    {
    }
}
