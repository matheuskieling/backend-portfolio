using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Profiles;

public sealed record DeleteProfileCommand(Guid ProfileId);

public sealed class DeleteProfileHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public DeleteProfileHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        ISchedulingUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(
        DeleteProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _profileRepository.GetByIdAsync(command.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(command.ProfileId);

        if (!profile.IsOwnedBy(userId))
            throw UnauthorizedSchedulingAccessException.NotProfileOwner();

        var hasAppointments = await _profileRepository.HasAppointmentsAsync(command.ProfileId, cancellationToken);
        if (hasAppointments)
            throw new CannotDeleteProfileException();

        _profileRepository.Remove(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
