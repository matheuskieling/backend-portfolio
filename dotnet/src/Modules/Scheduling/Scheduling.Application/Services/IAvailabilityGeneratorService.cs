using Scheduling.Domain.Entities;

namespace Scheduling.Application.Services;

public interface IAvailabilityGeneratorService
{
    Task<IReadOnlyList<Availability>> GenerateFromScheduleAsync(
        Schedule schedule,
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}
