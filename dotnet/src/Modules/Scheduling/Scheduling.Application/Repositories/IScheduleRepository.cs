using Scheduling.Domain.Entities;

namespace Scheduling.Application.Repositories;

public interface IScheduleRepository
{
    Task<Schedule?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Schedule>> GetByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default);
    Task<bool> ExistsNameAsync(Guid profileId, string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsNameExcludingAsync(Guid profileId, string name, Guid excludeScheduleId, CancellationToken cancellationToken = default);
    void Add(Schedule schedule);
    void Update(Schedule schedule);
    void Remove(Schedule schedule);
}
