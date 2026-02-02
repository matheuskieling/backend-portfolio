using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Availabilities;

public sealed record DeleteAvailabilityCommand(Guid ProfileId, Guid AvailabilityId);

public sealed class DeleteAvailabilityHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly IAvailabilityRepository _availabilityRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public DeleteAvailabilityHandler(
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

    public async Task HandleAsync(
        DeleteAvailabilityCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(command.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var availability = await _availabilityRepository.GetByIdWithTimeSlotsAsync(command.AvailabilityId, cancellationToken)
            ?? throw new AvailabilityNotFoundException(command.AvailabilityId);

        if (availability.HostProfileId != command.ProfileId)
            throw new AvailabilityNotFoundException(command.AvailabilityId);

        if (!availability.CanBeDeleted())
            throw new CannotDeleteAvailabilityException();

        _availabilityRepository.Remove(availability);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
