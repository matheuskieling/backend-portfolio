using Common.Domain;

namespace DocumentManager.Domain.Entities;

public sealed class ApprovalStep : BaseEntity
{
    public Guid WorkflowId { get; private set; }
    public ApprovalWorkflow Workflow { get; private set; } = null!;
    public int StepOrder { get; private set; }
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public string RequiredRole { get; private set; } = null!;

    private ApprovalStep() : base() { }

    private ApprovalStep(
        ApprovalWorkflow workflow,
        int stepOrder,
        string requiredRole,
        string? name,
        string? description) : base()
    {
        WorkflowId = workflow.Id;
        Workflow = workflow;
        StepOrder = stepOrder;
        RequiredRole = requiredRole;
        Name = name;
        Description = description;
    }

    public static ApprovalStep Create(
        ApprovalWorkflow workflow,
        int stepOrder,
        string requiredRole,
        string? name = null,
        string? description = null)
    {
        if (stepOrder < 1)
            throw new ArgumentException("Step order must be at least 1.", nameof(stepOrder));

        if (string.IsNullOrWhiteSpace(requiredRole))
            throw new ArgumentException("Required role cannot be empty.", nameof(requiredRole));

        return new ApprovalStep(workflow, stepOrder, requiredRole.Trim(), name?.Trim(), description?.Trim());
    }
}
