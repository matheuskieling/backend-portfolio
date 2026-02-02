using Identity.Application.Common.Interfaces;
using Scheduling.Application.Repositories;
using Scheduling.Domain.Enums;

namespace Scheduling.Application.UseCases.Profiles;

public sealed record GetMyProfilesQuery;

public sealed record ProfileDto(
    Guid Id,
    ProfileType Type,
    string? DisplayName,
    string? BusinessName,
    DateTime CreatedAt);

public sealed class GetMyProfilesHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISchedulingProfileRepository _profileRepository;

    public GetMyProfilesHandler(
        ICurrentUserService currentUserService,
        ISchedulingProfileRepository profileRepository)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
    }

    public async Task<IReadOnlyList<ProfileDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var profiles = await _profileRepository.GetByExternalUserIdAsync(userId, cancellationToken);

        return profiles.Select(p => new ProfileDto(
            p.Id,
            p.Type,
            p.DisplayName,
            p.BusinessName,
            p.CreatedAt)).ToList();
    }
}
