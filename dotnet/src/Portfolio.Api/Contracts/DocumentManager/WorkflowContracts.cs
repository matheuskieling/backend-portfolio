namespace Portfolio.Api.Contracts.DocumentManager;

/// <summary>
/// Request model for creating a new approval workflow.
/// </summary>
/// <param name="Name">The workflow name.</param>
/// <param name="Description">Optional description of the workflow.</param>
/// <param name="Steps">The approval steps in order.</param>
public sealed record CreateWorkflowRequest(
    string Name,
    string? Description,
    List<CreateWorkflowStepRequest> Steps);

/// <summary>
/// Request model for defining a workflow step.
/// </summary>
/// <param name="StepOrder">The order of this step (must be unique within the workflow).</param>
/// <param name="RequiredRole">The role required to approve this step.</param>
/// <param name="Name">Optional name for the step.</param>
/// <param name="Description">Optional description of the step.</param>
public sealed record CreateWorkflowStepRequest(
    int StepOrder,
    string RequiredRole,
    string? Name,
    string? Description);

/// <summary>
/// Response model for workflow creation.
/// </summary>
/// <param name="Id">The unique identifier of the created workflow.</param>
/// <param name="Name">The workflow name.</param>
/// <param name="StepCount">The number of steps in the workflow.</param>
public sealed record CreateWorkflowResponse(
    Guid Id,
    string Name,
    int StepCount);
