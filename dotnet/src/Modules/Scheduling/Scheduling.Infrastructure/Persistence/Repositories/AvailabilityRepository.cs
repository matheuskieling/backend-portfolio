using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Entities;

namespace Scheduling.Infrastructure.Persistence.Repositories;

public class AvailabilityRepository : IAvailabilityRepository
{
    private readonly SchedulingDbContext _context;

    public AvailabilityRepository(SchedulingDbContext context)
    {
        _context = context;
    }

    public async Task<Availability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Availabilities
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Availability?> GetByIdWithTimeSlotsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Availabilities
            .Include(a => a.TimeSlots)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Availability>> GetByProfileIdAsync(
        Guid profileId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Availabilities
            .Include(a => a.TimeSlots)
            .Where(a => a.HostProfileId == profileId);

        if (from.HasValue)
            query = query.Where(a => a.EndTime >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.StartTime <= to.Value);

        return await query
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Availability>> GetByScheduleIdAsync(
        Guid scheduleId, CancellationToken cancellationToken = default)
    {
        return await _context.Availabilities
            .Where(a => a.ScheduleId == scheduleId)
            .OrderBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasOverlappingAsync(
        Guid profileId, DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken cancellationToken = default)
    {
        return await _context.Availabilities
            .AnyAsync(a => a.HostProfileId == profileId
                && a.StartTime < endTime
                && a.EndTime > startTime, cancellationToken);
    }

    public async Task<bool> HasOverlappingExcludingAsync(
        Guid profileId, DateTimeOffset startTime, DateTimeOffset endTime, Guid excludeAvailabilityId, CancellationToken cancellationToken = default)
    {
        return await _context.Availabilities
            .AnyAsync(a => a.HostProfileId == profileId
                && a.Id != excludeAvailabilityId
                && a.StartTime < endTime
                && a.EndTime > startTime, cancellationToken);
    }

    public void Add(Availability availability)
    {
        _context.Availabilities.Add(availability);
    }

    public void Update(Availability availability)
    {
        _context.Availabilities.Update(availability);
    }

    public void Remove(Availability availability)
    {
        _context.Availabilities.Remove(availability);
    }
}
