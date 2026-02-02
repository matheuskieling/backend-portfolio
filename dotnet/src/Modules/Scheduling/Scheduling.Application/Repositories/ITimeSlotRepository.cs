using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Repositories;

public interface ITimeSlotRepository
{
    Task<TimeSlot?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TimeSlot?> GetByIdWithAvailabilityAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeSlot>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeSlot>> GetAvailableByProfileIdAsync(Guid profileId, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);
    void Update(TimeSlot timeSlot);
}
