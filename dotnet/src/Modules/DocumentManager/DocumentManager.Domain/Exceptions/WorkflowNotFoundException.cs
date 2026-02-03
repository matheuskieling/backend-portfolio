using Common.Domain.Exceptions;

namespace DocumentManager.Domain.Exceptions;

public sealed class WorkflowNotFoundException : NotFoundException
{
    private const string ErrorCode = "WORKFLOW_NOT_FOUND";

    public WorkflowNotFoundException(Guid workflowId)
        : base(ErrorCode, $"Workflow with ID '{workflowId}' was not found.")
    {
    }
}
