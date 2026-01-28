using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Enums;
using DocumentManager.Domain.Exceptions;
using Identity.Application.Common.Interfaces;

namespace DocumentManager.Application.UseCases.Approvals;

public sealed record RejectStepCommand(
    Guid ApprovalRequestId,
    string? Comment);

public sealed record RejectStepResult(
    Guid ApprovalRequestId,
    int StepRejected,
    ApprovalRequestStatus NewStatus);

public sealed class RejectStepHandler
{
    private readonly IApprovalRequestRepository _approvalRequestRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RejectStepHandler(
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

    public async Task<RejectStepResult> HandleAsync(
        RejectStepCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User must be authenticated to reject a step.");

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

        var stepRejected = approvalRequest.CurrentStepOrder;
        approvalRequest.RecordRejection(currentStep, userId, command.Comment);

        _approvalRequestRepository.Update(approvalRequest);
        _documentRepository.Update(approvalRequest.Document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RejectStepResult(approvalRequest.Id, stepRejected, approvalRequest.Status);
    }
}
