using Common.Domain;
using DocumentManager.Domain.Enums;
using DocumentManager.Domain.Exceptions;

namespace DocumentManager.Domain.Entities;

public sealed class ApprovalRequest : AuditableEntity, IAggregateRoot
{
    public Guid DocumentId { get; private set; }
    public Document Document { get; private set; } = null!;
    public Guid WorkflowId { get; private set; }
    public ApprovalWorkflow Workflow { get; private set; } = null!;
    public int CurrentStepOrder { get; private set; }
    public ApprovalRequestStatus Status { get; private set; }
    public Guid? RequestedBy { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private readonly List<ApprovalDecision> _decisions = new();
    public IReadOnlyCollection<ApprovalDecision> Decisions => _decisions.AsReadOnly();

    private ApprovalRequest() : base() { }

    private ApprovalRequest(Document document, ApprovalWorkflow workflow, Guid? requestedBy) : base()
    {
        DocumentId = document.Id;
        Document = document;
        WorkflowId = workflow.Id;
        Workflow = workflow;
        CurrentStepOrder = 1;
        Status = ApprovalRequestStatus.InProgress;
        RequestedBy = requestedBy;
        RequestedAt = DateTime.UtcNow;
    }

    public static ApprovalRequest Create(Document document, ApprovalWorkflow workflow, Guid? requestedBy)
    {
        workflow.EnsureIsActive();

        if (workflow.GetTotalSteps() == 0)
            throw new InvalidOperationException("Workflow must have at least one step.");

        return new ApprovalRequest(document, workflow, requestedBy);
    }

    public ApprovalStep? GetCurrentStep()
    {
        return Workflow.GetStep(CurrentStepOrder);
    }

    public ApprovalDecision RecordApproval(ApprovalStep step, Guid decidedBy, string? comment = null)
    {
        EnsureIsInProgress();
        EnsureCorrectStep(step);

        var decision = ApprovalDecision.CreateApproval(this, step, decidedBy, comment);
        _decisions.Add(decision);

        // Move to next step or complete if this was the last step
        var totalSteps = Workflow.GetTotalSteps();
        if (CurrentStepOrder >= totalSteps)
        {
            Status = ApprovalRequestStatus.Approved;
            CompletedAt = DateTime.UtcNow;
            Document.Approve();
        }
        else
        {
            CurrentStepOrder++;
        }

        SetUpdated();
        return decision;
    }

    public ApprovalDecision RecordRejection(ApprovalStep step, Guid decidedBy, string? comment = null)
    {
        EnsureIsInProgress();
        EnsureCorrectStep(step);

        var decision = ApprovalDecision.CreateRejection(this, step, decidedBy, comment);
        _decisions.Add(decision);

        Status = ApprovalRequestStatus.Rejected;
        CompletedAt = DateTime.UtcNow;
        Document.Reject();

        SetUpdated();
        return decision;
    }

    public bool IsStepCompleted(int stepOrder)
    {
        return _decisions.Any(d => d.Step.StepOrder == stepOrder);
    }

    public ApprovalDecision? GetDecisionForStep(int stepOrder)
    {
        return _decisions.FirstOrDefault(d => d.Step.StepOrder == stepOrder);
    }

    private void EnsureIsInProgress()
    {
        if (Status != ApprovalRequestStatus.InProgress)
            throw new ApprovalRequestNotInProgressException(Id, Status);
    }

    private void EnsureCorrectStep(ApprovalStep step)
    {
        if (step.StepOrder != CurrentStepOrder)
            throw new ApprovalStepOrderViolationException(CurrentStepOrder, step.StepOrder);
    }
}
