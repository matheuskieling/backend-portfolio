namespace DocumentManager.Application.DTOs;

public sealed record WorkflowDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    IReadOnlyList<WorkflowStepDto> Steps);

public sealed record WorkflowStepDto(
    Guid Id,
    int StepOrder,
    string? Name,
    string? Description,
    string RequiredRole);
