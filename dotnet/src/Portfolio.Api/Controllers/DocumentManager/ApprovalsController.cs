using Common.Contracts;
using DocumentManager.Application.DTOs;
using DocumentManager.Application.UseCases.Approvals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.DocumentManager;

namespace Portfolio.Api.Controllers.DocumentManager;

/// <summary>
/// Manages document approval processes including submission, approval, and rejection.
/// </summary>
[ApiController]
[Route("api/document-manager")]
[Tags("Document Manager - Approvals")]
[Authorize]
[Produces("application/json")]
public class ApprovalsController : ControllerBase
{
    private readonly SubmitForApprovalHandler _submitForApprovalHandler;
    private readonly ApproveStepHandler _approveStepHandler;
    private readonly RejectStepHandler _rejectStepHandler;
    private readonly GetApprovalStatusHandler _getApprovalStatusHandler;

    public ApprovalsController(
        SubmitForApprovalHandler submitForApprovalHandler,
        ApproveStepHandler approveStepHandler,
        RejectStepHandler rejectStepHandler,
        GetApprovalStatusHandler getApprovalStatusHandler)
    {
        _submitForApprovalHandler = submitForApprovalHandler;
        _approveStepHandler = approveStepHandler;
        _rejectStepHandler = rejectStepHandler;
        _getApprovalStatusHandler = getApprovalStatusHandler;
    }

    /// <summary>
    /// Submits a document for approval using a specified workflow.
    /// </summary>
    /// <remarks>
    /// The document must:
    /// - Be in Draft status
    /// - Have at least one version uploaded
    /// - Be owned by the current user
    /// </remarks>
    /// <param name="documentId">The document ID to submit.</param>
    /// <param name="request">The workflow to use for approval.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created approval request.</returns>
    /// <response code="201">Document successfully submitted for approval.</response>
    /// <response code="400">Document is not in draft status or has no versions.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to submit this document.</response>
    /// <response code="404">Document or workflow not found.</response>
    [HttpPost("documents/{documentId:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<SubmitForApprovalResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<SubmitForApprovalResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<SubmitForApprovalResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<SubmitForApprovalResponse>> SubmitForApproval(
        Guid documentId,
        [FromBody] SubmitForApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitForApprovalCommand(documentId, request.WorkflowId);
        var result = await _submitForApprovalHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Created(new SubmitForApprovalResponse(
            result.ApprovalRequestId,
            result.DocumentId,
            result.DocumentTitle));
    }

    /// <summary>
    /// Approves the current step in an approval workflow.
    /// </summary>
    /// <remarks>
    /// If this is the last step, the document status will change to Approved.
    /// Otherwise, the workflow advances to the next step.
    /// </remarks>
    /// <param name="approvalRequestId">The approval request ID.</param>
    /// <param name="request">Optional comment for the approval.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the approval action.</returns>
    /// <response code="200">Step successfully approved.</response>
    /// <response code="400">Approval request is not in progress.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to approve this step.</response>
    /// <response code="404">Approval request not found.</response>
    [HttpPost("approvals/{approvalRequestId:guid}/approve")]
    [ProducesResponseType(typeof(ApiResponse<ApproveStepResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ApproveStepResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ApproveStepResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<ApproveStepResponse>> Approve(
        Guid approvalRequestId,
        [FromBody] ApprovalActionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApproveStepCommand(approvalRequestId, request.Comment);
        var result = await _approveStepHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Success(new ApproveStepResponse(
            result.ApprovalRequestId,
            result.StepApproved,
            result.NewStatus.ToString()));
    }

    /// <summary>
    /// Rejects the current step in an approval workflow.
    /// </summary>
    /// <remarks>
    /// Rejection immediately ends the workflow and sets the document status to Rejected.
    /// </remarks>
    /// <param name="approvalRequestId">The approval request ID.</param>
    /// <param name="request">Optional comment explaining the rejection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the rejection action.</returns>
    /// <response code="200">Step successfully rejected.</response>
    /// <response code="400">Approval request is not in progress.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Not authorized to reject this step.</response>
    /// <response code="404">Approval request not found.</response>
    [HttpPost("approvals/{approvalRequestId:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<RejectStepResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<RejectStepResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<RejectStepResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<RejectStepResponse>> Reject(
        Guid approvalRequestId,
        [FromBody] ApprovalActionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RejectStepCommand(approvalRequestId, request.Comment);
        var result = await _rejectStepHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Success(new RejectStepResponse(
            result.ApprovalRequestId,
            result.StepRejected,
            result.NewStatus.ToString()));
    }

    /// <summary>
    /// Retrieves the current status of an approval request.
    /// </summary>
    /// <param name="approvalRequestId">The approval request ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The approval request status and details.</returns>
    /// <response code="200">Successfully retrieved approval status.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Approval request not found.</response>
    [HttpGet("approvals/{approvalRequestId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ApprovalRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<ApprovalRequestDto>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<ApprovalRequestDto>> GetStatus(
        Guid approvalRequestId,
        CancellationToken cancellationToken)
    {
        var query = new GetApprovalStatusQuery(approvalRequestId);
        var result = await _getApprovalStatusHandler.HandleAsync(query, cancellationToken);

        return ApiResponse.Success(result);
    }
}
