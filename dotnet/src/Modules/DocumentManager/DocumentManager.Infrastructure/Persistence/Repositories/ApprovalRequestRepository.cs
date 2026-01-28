using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Entities;
using DocumentManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Infrastructure.Persistence.Repositories;

public class ApprovalRequestRepository : IApprovalRequestRepository
{
    private readonly DocumentManagerDbContext _context;

    public ApprovalRequestRepository(DocumentManagerDbContext context)
    {
        _context = context;
    }

    public async Task<ApprovalRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);
    }

    public async Task<ApprovalRequest?> GetByIdWithDecisionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .Include(ar => ar.Decisions)
                .ThenInclude(d => d.Step)
            .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);
    }

    public async Task<ApprovalRequest?> GetByIdWithAllAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .Include(ar => ar.Document)
            .Include(ar => ar.Workflow)
                .ThenInclude(w => w.Steps)
            .Include(ar => ar.Decisions)
                .ThenInclude(d => d.Step)
            .FirstOrDefaultAsync(ar => ar.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ApprovalRequest>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .Include(ar => ar.Workflow)
            .Include(ar => ar.Decisions)
            .Where(ar => ar.DocumentId == documentId)
            .OrderByDescending(ar => ar.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApprovalRequest?> GetActiveByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .Include(ar => ar.Workflow)
                .ThenInclude(w => w.Steps)
            .Include(ar => ar.Decisions)
            .Where(ar => ar.DocumentId == documentId && ar.Status == ApprovalRequestStatus.InProgress)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApprovalRequest>> GetByStatusAsync(ApprovalRequestStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .Include(ar => ar.Document)
            .Include(ar => ar.Workflow)
            .Where(ar => ar.Status == status)
            .OrderByDescending(ar => ar.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApprovalRequest>> GetPendingForRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests
            .Include(ar => ar.Document)
            .Include(ar => ar.Workflow)
                .ThenInclude(w => w.Steps)
            .Where(ar => ar.Status == ApprovalRequestStatus.InProgress &&
                         ar.Workflow.Steps.Any(s =>
                             s.StepOrder == ar.CurrentStepOrder &&
                             s.RequiredRole == role))
            .OrderBy(ar => ar.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalRequests.AnyAsync(ar => ar.Id == id, cancellationToken);
    }

    public void Add(ApprovalRequest approvalRequest)
    {
        _context.ApprovalRequests.Add(approvalRequest);
    }

    public void Update(ApprovalRequest approvalRequest)
    {
        _context.ApprovalRequests.Update(approvalRequest);
    }
}
