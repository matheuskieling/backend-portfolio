using DocumentManager.Domain.Entities;
using DocumentManager.Domain.Enums;

namespace DocumentManager.Application.Repositories;

public interface IApprovalRequestRepository
{
    Task<ApprovalRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApprovalRequest?> GetByIdWithDecisionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApprovalRequest?> GetByIdWithAllAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalRequest>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<ApprovalRequest?> GetActiveByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalRequest>> GetByStatusAsync(ApprovalRequestStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalRequest>> GetPendingForRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(ApprovalRequest approvalRequest);
    void Update(ApprovalRequest approvalRequest);
}
