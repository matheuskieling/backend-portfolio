using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Entities;
using DocumentManager.Domain.Exceptions;
using Identity.Application.Common.Interfaces;

namespace DocumentManager.Application.UseCases.Approvals;

public sealed record SubmitForApprovalCommand(
    Guid DocumentId,
    Guid WorkflowId);

public sealed record SubmitForApprovalResult(
    Guid ApprovalRequestId,
    Guid DocumentId,
    string DocumentTitle);

public sealed class SubmitForApprovalHandler
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IApprovalWorkflowRepository _workflowRepository;
    private readonly IApprovalRequestRepository _approvalRequestRepository;
    private readonly IDocumentManagerUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SubmitForApprovalHandler(
        IDocumentRepository documentRepository,
        IApprovalWorkflowRepository workflowRepository,
        IApprovalRequestRepository approvalRequestRepository,
        IDocumentManagerUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _documentRepository = documentRepository;
        _workflowRepository = workflowRepository;
        _approvalRequestRepository = approvalRequestRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<SubmitForApprovalResult> HandleAsync(
        SubmitForApprovalCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new InvalidOperationException("User must be authenticated to submit a document for approval.");

        var document = await _documentRepository.GetByIdWithVersionsAsync(command.DocumentId, cancellationToken)
            ?? throw new DocumentNotFoundException(command.DocumentId);

        document.EnsureCanBeModifiedBy(userId);

        var workflow = await _workflowRepository.GetByIdWithStepsAsync(command.WorkflowId, cancellationToken)
            ?? throw new WorkflowNotFoundException(command.WorkflowId);

        // Submit document for approval (validates state and versions)
        document.SubmitForApproval();

        // Create approval request
        var approvalRequest = ApprovalRequest.Create(document, workflow, userId);

        _approvalRequestRepository.Add(approvalRequest);
        _documentRepository.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmitForApprovalResult(approvalRequest.Id, document.Id, document.Title);
    }
}
