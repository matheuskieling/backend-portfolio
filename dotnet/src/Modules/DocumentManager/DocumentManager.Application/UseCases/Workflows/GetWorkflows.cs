using DocumentManager.Application.DTOs;
using DocumentManager.Application.Repositories;

namespace DocumentManager.Application.UseCases.Workflows;

public sealed record GetWorkflowsQuery(bool ActiveOnly = true);

public sealed class GetWorkflowsHandler
{
    private readonly IApprovalWorkflowRepository _workflowRepository;

    public GetWorkflowsHandler(IApprovalWorkflowRepository workflowRepository)
    {
        _workflowRepository = workflowRepository;
    }

    public async Task<IReadOnlyList<WorkflowDto>> HandleAsync(
        GetWorkflowsQuery query,
        CancellationToken cancellationToken = default)
    {
        var workflows = query.ActiveOnly
            ? await _workflowRepository.GetActiveAsync(cancellationToken)
            : await _workflowRepository.GetAllAsync(cancellationToken);

        return workflows.Select(w => new WorkflowDto(
            w.Id,
            w.Name,
            w.Description,
            w.IsActive,
            w.GetOrderedSteps()
                .Select(s => new WorkflowStepDto(
                    s.Id,
                    s.StepOrder,
                    s.Name,
                    s.Description,
                    s.RequiredRole))
                .ToList()
                .AsReadOnly()))
            .ToList()
            .AsReadOnly();
    }
}
