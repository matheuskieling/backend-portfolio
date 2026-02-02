using Common.Domain;
using Identity.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Persistence;

public class SchedulingDbContext : DbContext, ISchedulingUnitOfWork
{
    private readonly ICurrentUserService? _currentUserService;

    public DbSet<SchedulingProfile> SchedulingProfiles => Set<SchedulingProfile>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    public SchedulingDbContext(DbContextOptions<SchedulingDbContext> options)
        : base(options)
    {
    }

    public SchedulingDbContext(
        DbContextOptions<SchedulingDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dotnet_scheduling");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedulingDbContext).Assembly);
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
