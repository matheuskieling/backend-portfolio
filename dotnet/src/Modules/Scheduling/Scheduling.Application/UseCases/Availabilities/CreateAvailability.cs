using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Availabilities;

public sealed record CreateAvailabilityCommand(
    Guid ProfileId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int SlotDurationMinutes,
    int MinAdvanceBookingMinutes = 60,
    int MaxAdvanceBookingDays = 30,
    int CancellationDeadlineMinutes = 60);

public sealed record TimeSlotDto(
    Guid Id,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status);

public sealed record CreateAvailabilityResult(
    Guid Id,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    int SlotDurationMinutes,
    IReadOnlyList<TimeSlotDto> TimeSlots);

public sealed class CreateAvailabilityHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public CreateAvailabilityHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        IAvailabilityRepository availabilityRepository,
        ISchedulingUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _availabilityRepository = availabilityRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateAvailabilityResult> HandleAsync(
        CreateAvailabilityCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(command.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var hasOverlap = await _availabilityRepository.HasOverlappingAsync(
            command.ProfileId,
            command.StartTime,
            command.EndTime,
            cancellationToken);

        if (hasOverlap)
            throw new OverlappingAvailabilityException(command.StartTime, command.EndTime);

        var availability = Availability.Create(
            command.ProfileId,
            command.StartTime,
            command.EndTime,
            command.SlotDurationMinutes,
            command.MinAdvanceBookingMinutes,
            command.MaxAdvanceBookingDays,
            command.CancellationDeadlineMinutes);

        _availabilityRepository.Add(availability);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateAvailabilityResult(
            availability.Id,
            availability.StartTime,
            availability.EndTime,
            availability.SlotDurationMinutes,
            availability.TimeSlots.Select(s => new TimeSlotDto(
                s.Id,
                s.StartTime,
                s.EndTime,
                s.Status.ToString())).ToList());
    }
}
