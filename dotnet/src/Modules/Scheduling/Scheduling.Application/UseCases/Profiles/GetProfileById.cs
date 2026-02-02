using Scheduling.Application.Repositories;
using Scheduling.Domain.Enums;
using Scheduling.Domain.Exceptions;

namespace Scheduling.Application.UseCases.Profiles;

public sealed record GetProfileByIdQuery(Guid ProfileId);

public sealed record GetProfileByIdResult(
    Guid Id,
    ProfileType Type,
    string? DisplayName,
    string? BusinessName,
    DateTime CreatedAt);

public sealed class GetProfileByIdHandler
{
    private readonly ISchedulingProfileRepository _profileRepository;

    public GetProfileByIdHandler(ISchedulingProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    public async Task<GetProfileByIdResult> HandleAsync(
        GetProfileByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var profile = await _profileRepository.GetByIdAsync(query.ProfileId, cancellationToken)
            ?? throw new SchedulingProfileNotFoundException(query.ProfileId);

        return new GetProfileByIdResult(
            profile.Id,
            profile.Type,
            profile.DisplayName,
            profile.BusinessName,
            profile.CreatedAt);
    }
}
