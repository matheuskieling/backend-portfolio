namespace DocumentManager.IntegrationTests.Infrastructure;

// DTOs for deserializing API read/list responses in tests.
// Create responses are in Portfolio.Api.Contracts.DocumentManager.

// Folders
public record FolderTreeResponse(Guid Id, string Name, IReadOnlyList<FolderTreeResponse> Children);

// Documents
public record DocumentDetailResponse(Guid Id, string Title, string? Description, string Status, int? CurrentVersion);
public record DocumentListResponse(Guid Id, string Title, string Status, DateTime CreatedAt);

// Workflows
public record WorkflowListResponse(Guid Id, string Name, string? Description, bool IsActive, int StepCount);

// Approvals
public record ApprovalStatusResponse(Guid Id, string Status, int CurrentStepOrder, int TotalSteps);

// Common
public record PagedResponse<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
