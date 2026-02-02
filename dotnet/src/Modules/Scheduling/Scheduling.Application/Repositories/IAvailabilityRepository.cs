using Scheduling.Domain.Entities;

namespace Scheduling.Application.Repositories;

public interface IAvailabilityRepository
{
    Task<Availability?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Availability?> GetByIdWithTimeSlotsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Availability>> GetByProfileIdAsync(Guid profileId, DateTimeOffset? from = null, DateTimeOffset? to = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Availability>> GetByScheduleIdAsync(Guid scheduleId, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingAsync(Guid profileId, DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken cancellationToken = default);
    Task<bool> HasOverlappingExcludingAsync(Guid profileId, DateTimeOffset startTime, DateTimeOffset endTime, Guid excludeAvailabilityId, CancellationToken cancellationToken = default);
    void Add(Availability availability);
    void Update(Availability availability);
    void Remove(Availability availability);
}
