using DocumentManager.Domain.Enums;

namespace DocumentManager.Application.DTOs;

public sealed record ApprovalRequestDto(
    Guid Id,
    Guid DocumentId,
    string DocumentTitle,
    Guid WorkflowId,
    string WorkflowName,
    int CurrentStepOrder,
    int TotalSteps,
    ApprovalRequestStatus Status,
    Guid? RequestedBy,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    IReadOnlyList<ApprovalDecisionDto> Decisions);

public sealed record ApprovalDecisionDto(
    Guid Id,
    int StepOrder,
    string? StepName,
    ApprovalDecisionType Decision,
    Guid? DecidedBy,
    string? Comment,
    DateTime DecidedAt);
