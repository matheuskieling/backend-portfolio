using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Persistence.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly SchedulingDbContext _context;

    public ScheduleRepository(SchedulingDbContext context)
    {
        _context = context;
    }

    public async Task<Schedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Schedules
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Schedule>> GetByProfileIdAsync(
        Guid profileId, CancellationToken cancellationToken = default)
    {
        return await _context.Schedules
            .Where(s => s.ProfileId == profileId)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsNameAsync(
        Guid profileId, string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await _context.Schedules
            .AnyAsync(s => s.ProfileId == profileId
                && s.Name.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<bool> ExistsNameExcludingAsync(
        Guid profileId, string name, Guid excludeScheduleId, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await _context.Schedules
            .AnyAsync(s => s.ProfileId == profileId
                && s.Id != excludeScheduleId
                && s.Name.ToLower() == normalizedName, cancellationToken);
    }

    public void Add(Schedule schedule)
    {
        _context.Schedules.Add(schedule);
    }

    public void Update(Schedule schedule)
    {
        _context.Schedules.Update(schedule);
    }

    public void Remove(Schedule schedule)
    {
        _context.Schedules.Remove(schedule);
    }
}
