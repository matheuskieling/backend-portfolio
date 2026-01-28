using Common.Domain;
using DocumentManager.Application.Common.Interfaces;
using DocumentManager.Domain.Entities;
using Identity.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Infrastructure.Persistence;

public class DocumentManagerDbContext : DbContext, IDocumentManagerUnitOfWork
{
    private readonly ICurrentUserService? _currentUserService;

    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<DocumentTag> DocumentTags => Set<DocumentTag>();
    public DbSet<ApprovalWorkflow> ApprovalWorkflows => Set<ApprovalWorkflow>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalDecision> ApprovalDecisions => Set<ApprovalDecision>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DocumentManagerDbContext(DbContextOptions<DocumentManagerDbContext> options)
        : base(options)
    {
    }

    public DocumentManagerDbContext(
        DbContextOptions<DocumentManagerDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dotnet_document_manager");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentManagerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        var currentUserId = _currentUserService?.UserId;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (currentUserId.HasValue)
                    {
                        entry.Entity.SetCreatedBy(currentUserId.Value);
                    }
                    break;

                case EntityState.Modified:
                    entry.Entity.SetUpdated(currentUserId);
                    break;
            }
        }
    }
}
