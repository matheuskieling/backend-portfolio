using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly SchedulingDbContext _context;

    public AppointmentRepository(SchedulingDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.TimeSlot)
            .Include(a => a.HostProfile)
            .Include(a => a.GuestProfile)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByProfileIdAsync(
        Guid profileId, AppointmentStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Appointments
            .Include(a => a.TimeSlot)
            .Where(a => a.HostProfileId == profileId || a.GuestProfileId == profileId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderByDescending(a => a.TimeSlot!.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByTimeSlotIdAsync(Guid timeSlotId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AnyAsync(a => a.TimeSlotId == timeSlotId && a.Status == AppointmentStatus.Scheduled, cancellationToken);
    }

    public void Add(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
    }

    public void Update(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
    }
}
