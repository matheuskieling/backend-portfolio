using Common.Domain;
using DocumentManager.Domain.Enums;

namespace DocumentManager.Domain.Entities;

public sealed class ApprovalDecision : BaseEntity
{
    public Guid ApprovalRequestId { get; private set; }
    public ApprovalRequest ApprovalRequest { get; private set; } = null!;
    public Guid StepId { get; private set; }
    public ApprovalStep Step { get; private set; } = null!;
    public Guid? DecidedBy { get; private set; }
    public ApprovalDecisionType Decision { get; private set; }
    public string? Comment { get; private set; }
    public DateTime DecidedAt { get; private set; }

    private ApprovalDecision() : base() { }

    private ApprovalDecision(
        ApprovalRequest approvalRequest,
        ApprovalStep step,
        Guid? decidedBy,
        ApprovalDecisionType decision,
        string? comment) : base()
    {
        ApprovalRequestId = approvalRequest.Id;
        ApprovalRequest = approvalRequest;
        StepId = step.Id;
        Step = step;
        DecidedBy = decidedBy;
        Decision = decision;
        Comment = comment;
        DecidedAt = DateTime.UtcNow;
    }

    public static ApprovalDecision CreateApproval(
        ApprovalRequest approvalRequest,
        ApprovalStep step,
        Guid decidedBy,
        string? comment = null)
    {
        return new ApprovalDecision(approvalRequest, step, decidedBy, ApprovalDecisionType.Approved, comment?.Trim());
    }

    public static ApprovalDecision CreateRejection(
        ApprovalRequest approvalRequest,
        ApprovalStep step,
        Guid decidedBy,
        string? comment = null)
    {
        return new ApprovalDecision(approvalRequest, step, decidedBy, ApprovalDecisionType.Rejected, comment?.Trim());
    }
}
