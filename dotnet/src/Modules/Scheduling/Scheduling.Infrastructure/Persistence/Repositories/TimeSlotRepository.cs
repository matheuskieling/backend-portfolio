using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Infrastructure.Persistence.Repositories;

public class TimeSlotRepository : ITimeSlotRepository
{
    private readonly SchedulingDbContext _context;

    public TimeSlotRepository(SchedulingDbContext context)
    {
        _context = context;
    }

    public async Task<TimeSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TimeSlots
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<TimeSlot?> GetByIdWithAvailabilityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TimeSlots
            .Include(t => t.Availability)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TimeSlot>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.TimeSlots
            .Include(t => t.Availability)
            .Where(t => idList.Contains(t.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TimeSlot>> GetAvailableByProfileIdAsync(
        Guid profileId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        return await _context.TimeSlots
            .Include(t => t.Availability)
            .Where(t => t.Availability!.HostProfileId == profileId
                && t.Status == TimeSlotStatus.Available
                && t.StartTime >= from
                && t.EndTime <= to)
            .OrderBy(t => t.StartTime)
            .ToListAsync(cancellationToken);
    }

    public void Update(TimeSlot timeSlot)
    {
        _context.TimeSlots.Update(timeSlot);
    }
}
