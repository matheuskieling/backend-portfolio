using Common.Domain;
using DocumentManager.Domain.Exceptions;

namespace DocumentManager.Domain.Entities;

public sealed class ApprovalWorkflow : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<ApprovalStep> _steps = new();
    public IReadOnlyCollection<ApprovalStep> Steps => _steps.AsReadOnly();

    private ApprovalWorkflow() : base() { }

    private ApprovalWorkflow(string name, string? description) : base()
    {
        Name = name;
        Description = description;
        IsActive = true;
    }

    public static ApprovalWorkflow Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Workflow name cannot be empty.", nameof(name));

        return new ApprovalWorkflow(name.Trim(), description?.Trim());
    }

    public ApprovalStep AddStep(int stepOrder, string requiredRole, string? name = null, string? description = null)
    {
        if (_steps.Any(s => s.StepOrder == stepOrder))
            throw new DuplicateStepOrderException(stepOrder, Id);

        if (string.IsNullOrWhiteSpace(requiredRole))
            throw new ArgumentException("Required role cannot be empty.", nameof(requiredRole));

        var step = ApprovalStep.Create(this, stepOrder, requiredRole, name, description);
        _steps.Add(step);
        SetUpdated();

        return step;
    }

    public ApprovalStep? GetStep(int stepOrder)
    {
        return _steps.FirstOrDefault(s => s.StepOrder == stepOrder);
    }

    public ApprovalStep? GetStepById(Guid stepId)
    {
        return _steps.FirstOrDefault(s => s.Id == stepId);
    }

    public int GetTotalSteps()
    {
        return _steps.Count;
    }

    public IReadOnlyList<ApprovalStep> GetOrderedSteps()
    {
        return _steps.OrderBy(s => s.StepOrder).ToList().AsReadOnly();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }

    public void EnsureIsActive()
    {
        if (!IsActive)
            throw new WorkflowNotActiveException(Id);
    }
}
