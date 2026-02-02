using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Application.Services;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Schedules;

public sealed record GenerateAvailabilitiesCommand(
    Guid ProfileId,
    Guid ScheduleId,
    DateOnly FromDate,
    DateOnly ToDate);

public sealed record GeneratedAvailabilityDto(
    Guid Id,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int SlotCount);

public sealed record GenerateAvailabilitiesResult(
    int GeneratedCount,
    int SkippedCount,
    IReadOnlyList<GeneratedAvailabilityDto> Availabilities);

public sealed class GenerateAvailabilitiesHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly IAvailabilityGeneratorService _generatorService;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public GenerateAvailabilitiesHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IScheduleRepository scheduleRepository,
        IAvailabilityRepository availabilityRepository,
        IAvailabilityGeneratorService generatorService,
        ISchedulingUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _scheduleRepository = scheduleRepository;
        _availabilityRepository = availabilityRepository;
        _generatorService = generatorService;
        _unitOfWork = unitOfWork;
    }

    public async Task<GenerateAvailabilitiesResult> HandleAsync(
        GenerateAvailabilitiesCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(command.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var schedule = await _scheduleRepository.GetByIdAsync(command.ScheduleId, cancellationToken)
            ?? throw new ScheduleNotFoundException(command.ScheduleId);

        if (schedule.ProfileId != command.ProfileId)
            throw new ScheduleNotFoundException(command.ScheduleId);

        if (!schedule.IsActive)
            throw new InvalidScheduleConfigurationException("Cannot generate availabilities from a paused schedule");

        var generatedAvailabilities = await _generatorService.GenerateFromScheduleAsync(
            schedule, command.FromDate, command.ToDate, cancellationToken);

        var addedAvailabilities = new List<GeneratedAvailabilityDto>();
        var skippedCount = 0;

        foreach (var availability in generatedAvailabilities)
        {
            var hasOverlap = await _availabilityRepository.HasOverlappingAsync(
                command.ProfileId,
                availability.StartTime,
                availability.EndTime,
                cancellationToken);

            if (hasOverlap)
            {
                skippedCount++;
                continue;
            }

            _availabilityRepository.Add(availability);
            addedAvailabilities.Add(new GeneratedAvailabilityDto(
                availability.Id,
                availability.StartTime,
                availability.EndTime,
                availability.TimeSlots.Count));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new GenerateAvailabilitiesResult(
            addedAvailabilities.Count,
            skippedCount,
            addedAvailabilities);
    }
}
