using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetByProfileIdAsync(Guid profileId, AppointmentStatus? status = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByTimeSlotIdAsync(Guid timeSlotId, CancellationToken cancellationToken = default);
    void Add(Appointment appointment);
    void Update(Appointment appointment);
}
