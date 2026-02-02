using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.Repositories;

public interface ISchedulingProfileRepository
{
    Task<SchedulingProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SchedulingProfile>> GetByExternalUserIdAsync(Guid externalUserId, CancellationToken cancellationToken = default);
    Task<bool> ExistsIndividualProfileAsync(Guid externalUserId, CancellationToken cancellationToken = default);
    Task<bool> ExistsBusinessNameAsync(Guid externalUserId, string businessName, CancellationToken cancellationToken = default);
    Task<bool> HasAppointmentsAsync(Guid profileId, CancellationToken cancellationToken = default);
    void Add(SchedulingProfile profile);
    void Update(SchedulingProfile profile);
    void Remove(SchedulingProfile profile);
}
