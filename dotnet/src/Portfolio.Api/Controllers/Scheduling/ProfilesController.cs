using Common.Contracts;
using Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.Scheduling;
using Scheduling.Application.UseCases.Profiles;
using Scheduling.Domain.Enums;

namespace Portfolio.Api.Controllers.Scheduling;

/// <summary>
/// Manages scheduling profiles for users.
/// </summary>
[ApiController]
[Route("api/scheduling/profiles")]
[Tags("Scheduling - Profiles")]
[Produces("application/json")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly CreateProfileHandler _createProfileHandler;
    private readonly GetMyProfilesHandler _getMyProfilesHandler;
    private readonly GetProfileByIdHandler _getProfileByIdHandler;
    private readonly DeleteProfileHandler _deleteProfileHandler;

    public ProfilesController(
        CreateProfileHandler createProfileHandler,
        GetMyProfilesHandler getMyProfilesHandler,
        GetProfileByIdHandler getProfileByIdHandler,
        DeleteProfileHandler deleteProfileHandler)
    {
        _createProfileHandler = createProfileHandler;
        _getMyProfilesHandler = getMyProfilesHandler;
        _getProfileByIdHandler = getProfileByIdHandler;
        _deleteProfileHandler = deleteProfileHandler;
    }

    /// <summary>
    /// Creates a new scheduling profile for the current user.
    /// </summary>
    /// <param name="request">The profile creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created profile.</returns>
    /// <response code="201">Profile successfully created.</response>
    /// <response code="400">Invalid request or domain validation error.</response>
    /// <response code="409">Profile already exists (duplicate Individual or Business name).</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status409Conflict)]
    public async Task<ApiResponse<ProfileResponse>> CreateProfile(
        [FromBody] CreateProfileRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!Enum.TryParse<ProfileType>(request.Type, true, out var profileType))
                return ApiResponse.Failure<ProfileResponse>("INVALID_PROFILE_TYPE", $"Invalid profile type: {request.Type}. Must be 'Individual' or 'Business'.");

            var command = new CreateProfileCommand(
                profileType,
                request.DisplayName,
                request.BusinessName);

            var result = await _createProfileHandler.HandleAsync(command, cancellationToken);

            return ApiResponse.Created(new ProfileResponse(
                result.Id,
                result.Type.ToString(),
                result.DisplayName,
                result.BusinessName,
                DateTime.UtcNow));
        }
        catch (DomainException ex)
        {
            return ApiResponse.Failure<ProfileResponse>(ex.Code, ex.Message);
        }
    }

    /// <summary>
    /// Lists all scheduling profiles for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of profiles owned by the current user.</returns>
    /// <response code="200">Profiles retrieved successfully.</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProfileResponse>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse<IReadOnlyList<ProfileResponse>>> GetMyProfiles(
        CancellationToken cancellationToken)
    {
        var result = await _getMyProfilesHandler.HandleAsync(cancellationToken);

        var profiles = result.Select(p => new ProfileResponse(
            p.Id,
            p.Type.ToString(),
            p.DisplayName,
            p.BusinessName,
            p.CreatedAt)).ToList();

        return ApiResponse.Success<IReadOnlyList<ProfileResponse>>(profiles);
    }

    /// <summary>
    /// Gets a scheduling profile by ID (public information).
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The profile information.</returns>
    /// <response code="200">Profile retrieved successfully.</response>
    /// <response code="404">Profile not found.</response>
    [HttpGet("{profileId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProfileResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<ProfileResponse>> GetProfileById(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetProfileByIdQuery(profileId);
            var result = await _getProfileByIdHandler.HandleAsync(query, cancellationToken);

            return ApiResponse.Success(new ProfileResponse(
                result.Id,
                result.Type.ToString(),
                result.DisplayName,
                result.BusinessName,
                result.CreatedAt));
        }
        catch (DomainException ex)
        {
            return ApiResponse.NotFound<ProfileResponse>(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a scheduling profile.
    /// </summary>
    /// <param name="profileId">The profile ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Profile successfully deleted.</response>
    /// <response code="400">Cannot delete profile with existing appointments.</response>
    /// <response code="403">Not authorized to delete this profile.</response>
    /// <response code="404">Profile not found.</response>
    [HttpDelete("{profileId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProfile(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteProfileCommand(profileId);
            await _deleteProfileHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
        catch (DomainException ex)
        {
            if (ex.Code == "SCHEDULING_PROFILE_NOT_FOUND")
                return ApiResponse.NotFound<object>(ex.Message);
            if (ex.Code == "UNAUTHORIZED_SCHEDULING_ACCESS")
                return ApiResponse.Forbidden<object>(ex.Message);
            return ApiResponse.Failure<object>(ex.Code, ex.Message);
        }
    }
}
