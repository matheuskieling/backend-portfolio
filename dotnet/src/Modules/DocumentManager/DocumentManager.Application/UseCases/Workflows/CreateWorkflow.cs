using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Entities;

namespace DocumentManager.Application.UseCases.Workflows;

public sealed record CreateWorkflowCommand(
    string Name,
    string? Description,
    IReadOnlyList<CreateWorkflowStepCommand> Steps);

public sealed record CreateWorkflowStepCommand(
    int StepOrder,
    string RequiredRole,
    string? Name,
    string? Description);

public sealed record CreateWorkflowResult(
    Guid Id,
    string Name,
    int StepCount);

public sealed class CreateWorkflowHandler
{
    private readonly IApprovalWorkflowRepository _workflowRepository;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;

    public CreateWorkflowHandler(
        IApprovalWorkflowRepository workflowRepository,
        IDocumentManagerUnitOfWork unitOfWork)
    {
        _workflowRepository = workflowRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateWorkflowResult> HandleAsync(
        CreateWorkflowCommand command,
        CancellationToken cancellationToken = default)
    {
        var workflow = ApprovalWorkflow.Create(command.Name, command.Description);

        foreach (var step in command.Steps.OrderBy(s => s.StepOrder))
        {
            workflow.AddStep(step.StepOrder, step.RequiredRole, step.Name, step.Description);
        }

        _workflowRepository.Add(workflow);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateWorkflowResult(workflow.Id, workflow.Name, workflow.GetTotalSteps());
    }
}
