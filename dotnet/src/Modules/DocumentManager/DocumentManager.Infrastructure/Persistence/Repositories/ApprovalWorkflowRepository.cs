using DocumentManager.Application.Repositories;
using DocumentManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Infrastructure.Persistence.Repositories;

public class ApprovalWorkflowRepository : IApprovalWorkflowRepository
{
    private readonly DocumentManagerDbContext _context;

    public ApprovalWorkflowRepository(DocumentManagerDbContext context)
    {
        _context = context;
    }

    public async Task<ApprovalWorkflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalWorkflows
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<ApprovalWorkflow?> GetByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalWorkflows
            .Include(w => w.Steps)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ApprovalWorkflow>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalWorkflows
            .Include(w => w.Steps)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApprovalWorkflow>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalWorkflows
            .Include(w => w.Steps)
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApprovalWorkflows.AnyAsync(w => w.Id == id, cancellationToken);
    }

    public void Add(ApprovalWorkflow workflow)
    {
        _context.ApprovalWorkflows.Add(workflow);
    }

    public void Update(ApprovalWorkflow workflow)
    {
        _context.ApprovalWorkflows.Update(workflow);
    }

    public void Remove(ApprovalWorkflow workflow)
    {
        _context.ApprovalWorkflows.Remove(workflow);
    }
}
