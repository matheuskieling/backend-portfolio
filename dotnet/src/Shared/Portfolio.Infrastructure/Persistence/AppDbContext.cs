using Identity.Application.Common.Interfaces;
using Identity.Domain.Common;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    private readonly ICurrentUserService? _currentUserService;

    // Identity module
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
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
