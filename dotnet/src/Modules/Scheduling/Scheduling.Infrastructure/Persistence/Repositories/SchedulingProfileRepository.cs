using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Infrastructure.Persistence.Repositories;

public class SchedulingProfileRepository : ISchedulingProfileRepository
{
    private readonly SchedulingDbContext _context;

    public SchedulingProfileRepository(SchedulingDbContext context)
    {
        _context = context;
    }

    public async Task<SchedulingProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SchedulingProfiles
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<SchedulingProfile>> GetByExternalUserIdAsync(
        Guid externalUserId, CancellationToken cancellationToken = default)
    {
        return await _context.SchedulingProfiles
            .Where(p => p.ExternalUserId == externalUserId)
            .OrderBy(p => p.Type)
            .ThenBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsIndividualProfileAsync(
        Guid externalUserId, CancellationToken cancellationToken = default)
    {
        return await _context.SchedulingProfiles
            .AnyAsync(p => p.ExternalUserId == externalUserId && p.Type == ProfileType.Individual, cancellationToken);
    }

    public async Task<bool> ExistsBusinessNameAsync(
        Guid externalUserId, string businessName, CancellationToken cancellationToken = default)
    {
        var normalizedName = businessName.Trim().ToLowerInvariant();
        return await _context.SchedulingProfiles
            .AnyAsync(p => p.ExternalUserId == externalUserId
                && p.Type == ProfileType.Business
                && p.BusinessName != null
                && p.BusinessName.ToLower() == normalizedName, cancellationToken);
    }

    public async Task<bool> HasAppointmentsAsync(Guid profileId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AnyAsync(a => (a.HostProfileId == profileId || a.GuestProfileId == profileId)
                && a.Status == AppointmentStatus.Scheduled, cancellationToken);
    }

    public void Add(SchedulingProfile profile)
    {
        _context.SchedulingProfiles.Add(profile);
    }

    public void Update(SchedulingProfile profile)
    {
        _context.SchedulingProfiles.Update(profile);
    }

    public void Remove(SchedulingProfile profile)
    {
        _context.SchedulingProfiles.Remove(profile);
    }
}
