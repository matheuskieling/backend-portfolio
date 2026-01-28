using DocumentManager.Application.DTOs;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Exceptions;

namespace DocumentManager.Application.UseCases.Approvals;

public sealed record GetApprovalStatusQuery(Guid ApprovalRequestId);

public sealed class GetApprovalStatusHandler
{
    private readonly IApprovalRequestRepository _approvalRequestRepository;

    public GetApprovalStatusHandler(IApprovalRequestRepository approvalRequestRepository)
    {
        _approvalRequestRepository = approvalRequestRepository;
    }

    public async Task<ApprovalRequestDto> HandleAsync(
        GetApprovalStatusQuery query,
        CancellationToken cancellationToken = default)
    {
        var approvalRequest = await _approvalRequestRepository.GetByIdWithAllAsync(query.ApprovalRequestId, cancellationToken)
            ?? throw new ApprovalRequestNotFoundException(query.ApprovalRequestId);

        var decisions = approvalRequest.Decisions
            .OrderBy(d => d.Step.StepOrder)
            .Select(d => new ApprovalDecisionDto(
                d.Id,
                d.Step.StepOrder,
                d.Step.Name,
                d.Decision,
                d.DecidedBy,
                d.Comment,
                d.DecidedAt))
            .ToList()
            .AsReadOnly();

        return new ApprovalRequestDto(
            approvalRequest.Id,
            approvalRequest.DocumentId,
            approvalRequest.Document.Title,
            approvalRequest.WorkflowId,
            approvalRequest.Workflow.Name,
            approvalRequest.CurrentStepOrder,
            approvalRequest.Workflow.GetTotalSteps(),
            approvalRequest.Status,
            approvalRequest.RequestedBy,
            approvalRequest.RequestedAt,
            approvalRequest.CompletedAt,
            decisions);
    }
}
