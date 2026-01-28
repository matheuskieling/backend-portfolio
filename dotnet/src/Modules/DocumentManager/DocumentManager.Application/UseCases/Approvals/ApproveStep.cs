using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Enums;
using DocumentManager.Domain.Exceptions;
using Identity.Application.Common.Interfaces;

namespace DocumentManager.Application.UseCases.Approvals;

public sealed record ApproveStepCommand(
    Guid ApprovalRequestId,
    string? Comment);

public sealed record ApproveStepResult(
    Guid ApprovalRequestId,
    int StepApproved,
    ApprovalRequestStatus NewStatus);

public sealed class ApproveStepHandler
{
    private readonly IApprovalRequestRepository _approvalRequestRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ApproveStepHandler(
        IApprovalRequestRepository approvalRequestRepository,
        IDocumentRepository documentRepository,
        IDocumentManagerUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _approvalRequestRepository = approvalRequestRepository;
        _documentRepository = documentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApproveStepResult> HandleAsync(
        ApproveStepCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User must be authenticated to approve a step.");

        var approvalRequest = await _approvalRequestRepository.GetByIdWithAllAsync(command.ApprovalRequestId, cancellationToken)
            ?? throw new ApprovalRequestNotFoundException(command.ApprovalRequestId);

        var currentStep = approvalRequest.GetCurrentStep()
            ?? throw new InvalidOperationException("No current step found for approval request.");

        // Check if user has the required role
        if (!_currentUserService.HasRole(currentStep.RequiredRole) &&
            !_currentUserService.HasPermission("approval:review"))
        {
            throw new UnauthorizedApproverException(userId, currentStep.RequiredRole);
        }

        var stepApproved = approvalRequest.CurrentStepOrder;
        approvalRequest.RecordApproval(currentStep, userId, command.Comment);

        _approvalRequestRepository.Update(approvalRequest);
        _documentRepository.Update(approvalRequest.Document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApproveStepResult(approvalRequest.Id, stepApproved, approvalRequest.Status);
    }
}
