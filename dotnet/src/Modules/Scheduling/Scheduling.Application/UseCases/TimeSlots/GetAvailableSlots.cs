using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.TimeSlots;

public sealed record GetAvailableSlotsQuery(
    Guid ProfileId,
    DateTimeOffset From,
    DateTimeOffset To);

public sealed record AvailableSlotDto(
    Guid Id,
    Guid AvailabilityId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime);

public sealed class GetAvailableSlotsHandler
{
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly ITimeSlotRepository _timeSlotRepository;

    public GetAvailableSlotsHandler(
        ISchedulingProfileRepository profileRepository,
        ITimeSlotRepository timeSlotRepository)
    {
        _profileRepository = profileRepository;
        _timeSlotRepository = timeSlotRepository;
    }

    public async Task<IReadOnlyList<AvailableSlotDto>> HandleAsync(
        GetAvailableSlotsQuery query,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByIdAsync(query.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(query.ProfileId);

        var slots = await _timeSlotRepository.GetAvailableByProfileIdAsync(
            query.ProfileId, query.From, query.To, cancellationToken);

        return slots.Select(s => new AvailableSlotDto(
            s.Id,
            s.AvailabilityId,
            s.StartTime,
            s.EndTime)).ToList();
    }
}
