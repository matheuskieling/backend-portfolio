using DocumentManager.Domain.Entities;

namespace DocumentManager.Application.Repositories;

public interface IApprovalWorkflowRepository
{
    Task<ApprovalWorkflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApprovalWorkflow?> GetByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalWorkflow>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalWorkflow>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(ApprovalWorkflow workflow);
    void Update(ApprovalWorkflow workflow);
    void Remove(ApprovalWorkflow workflow);
}
