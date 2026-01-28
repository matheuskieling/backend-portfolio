namespace Portfolio.Api.Contracts.DocumentManager;

/// <summary>
/// Request model for submitting a document for approval.
/// </summary>
/// <param name="WorkflowId">The ID of the workflow to use for approval.</param>
public sealed record SubmitForApprovalRequest(Guid WorkflowId);

/// <summary>
/// Request model for approval or rejection actions.
/// </summary>
/// <param name="Comment">Optional comment explaining the decision.</param>
public sealed record ApprovalActionRequest(string? Comment);

/// <summary>
/// Response model for document submission.
/// </summary>
/// <param name="ApprovalRequestId">The unique identifier of the created approval request.</param>
/// <param name="DocumentId">The document ID.</param>
/// <param name="DocumentTitle">The document title.</param>
public sealed record SubmitForApprovalResponse(
    Guid ApprovalRequestId,
    Guid DocumentId,
    string DocumentTitle);

/// <summary>
/// Response model for step approval.
/// </summary>
/// <param name="ApprovalRequestId">The approval request ID.</param>
/// <param name="StepApproved">The step number that was approved.</param>
/// <param name="NewStatus">The new status of the approval request (InProgress or Approved).</param>
public sealed record ApproveStepResponse(
    Guid ApprovalRequestId,
    int StepApproved,
    string NewStatus);

/// <summary>
/// Response model for step rejection.
/// </summary>
/// <param name="ApprovalRequestId">The approval request ID.</param>
/// <param name="StepRejected">The step number that was rejected.</param>
/// <param name="NewStatus">The new status (Rejected).</param>
public sealed record RejectStepResponse(
    Guid ApprovalRequestId,
    int StepRejected,
    string NewStatus);
