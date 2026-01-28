using Common.Contracts;
using DocumentManager.Application.DTOs;
using DocumentManager.Application.UseCases.Workflows;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.DocumentManager;

namespace Portfolio.Api.Controllers.DocumentManager;

/// <summary>
/// Manages approval workflows that define the steps required for document approval.
/// </summary>
[ApiController]
[Route("api/document-manager/workflows")]
[Tags("Document Manager - Workflows")]
[Authorize]
[Produces("application/json")]
public class WorkflowsController : ControllerBase
{
    private readonly CreateWorkflowHandler _createWorkflowHandler;
    private readonly GetWorkflowsHandler _getWorkflowsHandler;

    public WorkflowsController(
        CreateWorkflowHandler createWorkflowHandler,
        GetWorkflowsHandler getWorkflowsHandler)
    {
        _createWorkflowHandler = createWorkflowHandler;
        _getWorkflowsHandler = getWorkflowsHandler;
    }

    /// <summary>
    /// Creates a new approval workflow with defined steps.
    /// </summary>
    /// <param name="request">The workflow creation details including steps.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created workflow.</returns>
    /// <response code="201">Workflow successfully created.</response>
    /// <response code="400">Invalid request data (e.g., duplicate step orders, missing required fields).</response>
    /// <response code="401">Authentication required.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateWorkflowResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<CreateWorkflowResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResponse<CreateWorkflowResponse>> Create(
        [FromBody] CreateWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var steps = request.Steps.Select(s => new CreateWorkflowStepCommand(
            s.StepOrder,
            s.RequiredRole,
            s.Name,
            s.Description)).ToList();

        var command = new CreateWorkflowCommand(request.Name, request.Description, steps);
        var result = await _createWorkflowHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Created(new CreateWorkflowResponse(
            result.Id,
            result.Name,
            result.StepCount));
    }

    /// <summary>
    /// Retrieves all workflows, optionally filtered by active status.
    /// </summary>
    /// <param name="activeOnly">If true, returns only active workflows (default: true).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of workflows.</returns>
    /// <response code="200">Successfully retrieved workflows.</response>
    /// <response code="401">Authentication required.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkflowDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ApiResponse<IReadOnlyList<WorkflowDto>>> GetAll(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWorkflowsQuery(activeOnly);
        var result = await _getWorkflowsHandler.HandleAsync(query, cancellationToken);

        return ApiResponse.Success(result);
    }
}
