using Identity.Application.Common.Interfaces;
using Scheduling.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Entities;
using Scheduling.Domain.Enums;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Profiles;

public sealed record CreateProfileCommand(
    ProfileType Type,
    string? DisplayName,
    string? BusinessName);

public sealed record CreateProfileResult(
    Guid Id,
    ProfileType Type,
    string? DisplayName,
    string? BusinessName);

public sealed class CreateProfileHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;
    private readonly ISchedulingUnitOfWork _unitOfWork;

    public CreateProfileHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository,
        ISchedulingUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateProfileResult> HandleAsync(
        CreateProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        if (command.Type == ProfileType.Individual)
        {
            var hasIndividual = await _profileRepository.ExistsIndividualProfileAsync(userId, cancellationToken);
            if (hasIndividual)
                throw SchedulingProfileAlreadyExistsException.IndividualProfileExists(userId);
        }
        else if (command.Type == ProfileType.Business)
        {
            if (string.IsNullOrWhiteSpace(command.BusinessName))
                throw new InvalidScheduleConfigurationException("Business name is required for business profiles");

            var hasBusinessName = await _profileRepository.ExistsBusinessNameAsync(userId, command.BusinessName, cancellationToken);
            if (hasBusinessName)
                throw SchedulingProfileAlreadyExistsException.BusinessNameExists(command.BusinessName);
        }

        var profile = command.Type == ProfileType.Individual
            ? SchedulingProfile.CreateIndividual(userId, command.DisplayName)
            : SchedulingProfile.CreateBusiness(userId, command.BusinessName!, command.DisplayName);

        _profileRepository.Add(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProfileResult(
            profile.Id,
            profile.Type,
            profile.DisplayName,
            profile.BusinessName);
    }
}
