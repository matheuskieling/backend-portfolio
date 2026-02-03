using Common.Contracts;
using Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.Scheduling;
using Scheduling.Application.UseCases.Availabilities;

namespace Portfolio.Api.Controllers.Scheduling;

/// <summary>
/// Manages availabilities for scheduling profiles.
/// </summary>
[ApiController]
[Route("api/scheduling/profiles/{profileId:guid}/availabilities")]
[Tags("Scheduling - Availabilities")]
[Produces("application/json")]
[Authorize]
public class AvailabilitiesController : ControllerBase
{
    private readonly CreateAvailabilityHandler _createAvailabilityHandler;
    private readonly GetAvailabilitiesHandler _getAvailabilitiesHandler;
    private readonly GetAvailabilityByIdHandler _getAvailabilityByIdHandler;
    private readonly DeleteAvailabilityHandler _deleteAvailabilityHandler;

    public AvailabilitiesController(
        CreateAvailabilityHandler createAvailabilityHandler,
        GetAvailabilitiesHandler getAvailabilitiesHandler,
        GetAvailabilityByIdHandler getAvailabilityByIdHandler,
        DeleteAvailabilityHandler deleteAvailabilityHandler)
    {
        _createAvailabilityHandler = createAvailabilityHandler;
        _getAvailabilitiesHandler = getAvailabilitiesHandler;
        _getAvailabilityByIdHandler = getAvailabilityByIdHandler;
        _deleteAvailabilityHandler = deleteAvailabilityHandler;
    }

    /// <summary>
    /// Creates a single-occurrence availability for the profile.
    /// </summary>
    /// <param name="profileId">The profile ID to create availability for.</param>
    /// <param name="request">The availability creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created availability with time slots.</returns>
    /// <response code="201">Availability successfully created.</response>
    /// <response code="400">Invalid request or domain validation error.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to create availability for this profile.</response>
    /// <response code="404">Profile not found.</response>
    /// <response code="409">Overlapping availability exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityDetailResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityDetailResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityDetailResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityDetailResponse>), StatusCodes.Status409Conflict)]
    public async Task<ApiResponse<AvailabilityDetailResponse>> CreateAvailability(
        Guid profileId,
        [FromBody] CreateAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateAvailabilityCommand(
                profileId,
                request.StartTime,
                request.EndTime,
                request.SlotDurationMinutes,
                request.MinAdvanceBookingMinutes,
                request.MaxAdvanceBookingDays,
                request.CancellationDeadlineMinutes);

            var result = await _createAvailabilityHandler.HandleAsync(command, cancellationToken);

            return ApiResponse.Created(new AvailabilityDetailResponse(
                result.Id,
                null,
                result.StartTime,
                result.EndTime,
                result.SlotDurationMinutes,
                request.MinAdvanceBookingMinutes,
                request.MaxAdvanceBookingDays,
                request.CancellationDeadlineMinutes,
                result.TimeSlots.Select(s => new TimeSlotResponse(
                    s.Id,
                    s.StartTime,
                    s.EndTime,
                    s.Status)).ToList(),
                DateTime.UtcNow));
        }
        catch (DomainException ex)
        {
            return HandleDomainException<AvailabilityDetailResponse>(ex);
        }
    }

    /// <summary>
    /// Lists availabilities for the profile, optionally filtered by date range.
    /// </summary>
    /// <param name="profileId">The profile ID to list availabilities for.</param>
    /// <param name="from">Optional start of date range filter.</param>
    /// <param name="to">Optional end of date range filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of availabilities for the profile.</returns>
    /// <response code="200">Availabilities retrieved successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to view availabilities for this profile.</response>
    /// <response code="404">Profile not found.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AvailabilityResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AvailabilityResponse>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AvailabilityResponse>>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<IReadOnlyList<AvailabilityResponse>>> GetAvailabilities(
        Guid profileId,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetAvailabilitiesQuery(profileId, from, to);
            var result = await _getAvailabilitiesHandler.HandleAsync(query, cancellationToken);

            var availabilities = result.Select(a => new AvailabilityResponse(
                a.Id,
                a.ScheduleId,
                a.StartTime,
                a.EndTime,
                a.SlotDurationMinutes,
                a.TotalSlots,
                a.AvailableSlots,
                a.BookedSlots,
                a.BlockedSlots,
                a.CreatedAt)).ToList();

            return ApiResponse.Success<IReadOnlyList<AvailabilityResponse>>(availabilities);
        }
        catch (DomainException ex)
        {
            return HandleDomainException<IReadOnlyList<AvailabilityResponse>>(ex);
        }
    }

    /// <summary>
    /// Gets an availability with its time slots.
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="availabilityId">The availability ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The availability with its time slots.</returns>
    /// <response code="200">Availability retrieved successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to view this availability.</response>
    /// <response code="404">Profile or availability not found.</response>
    [HttpGet("{availabilityId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityDetailResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AvailabilityDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<AvailabilityDetailResponse>> GetAvailabilityById(
        Guid profileId,
        Guid availabilityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetAvailabilityByIdQuery(profileId, availabilityId);
            var result = await _getAvailabilityByIdHandler.HandleAsync(query, cancellationToken);

            return ApiResponse.Success(new AvailabilityDetailResponse(
                result.Id,
                result.ScheduleId,
                result.StartTime,
                result.EndTime,
                result.SlotDurationMinutes,
                result.MinAdvanceBookingMinutes,
                result.MaxAdvanceBookingDays,
                result.CancellationDeadlineMinutes,
                result.TimeSlots.Select(s => new TimeSlotResponse(
                    s.Id,
                    s.StartTime,
                    s.EndTime,
                    s.Status)).ToList(),
                result.CreatedAt));
        }
        catch (DomainException ex)
        {
            return HandleDomainException<AvailabilityDetailResponse>(ex);
        }
    }

    /// <summary>
    /// Deletes an availability (only if no slots are booked).
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="availabilityId">The availability ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Availability successfully deleted.</response>
    /// <response code="400">Cannot delete availability with booked slots.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to delete this availability.</response>
    /// <response code="404">Profile or availability not found.</response>
    [HttpDelete("{availabilityId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAvailability(
        Guid profileId,
        Guid availabilityId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteAvailabilityCommand(profileId, availabilityId);
            await _deleteAvailabilityHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return ex.Code switch
            {
                "SCHEDULING_PROFILE_NOT_FOUND" or "AVAILABILITY_NOT_FOUND" => ApiResponse.NotFound<object>(ex.Message),
                "UNAUTHORIZED_SCHEDULING_ACCESS" => ApiResponse.Forbidden<object>(ex.Message),
                _ => ApiResponse.Failure<object>(ex.Code, ex.Message)
            };
        }
    }

    private static ApiResponse<T> HandleDomainException<T>(DomainException ex)
    {
        return ex.Code switch
        {
            "SCHEDULING_PROFILE_NOT_FOUND" or "AVAILABILITY_NOT_FOUND" => ApiResponse.NotFound<T>(ex.Message),
            "UNAUTHORIZED_SCHEDULING_ACCESS" => ApiResponse.Forbidden<T>(ex.Message),
            "OVERLAPPING_AVAILABILITY" => ApiResponse.Failure<T>(ex.Code, ex.Message),
            _ => ApiResponse.Failure<T>(ex.Code, ex.Message)
        };
    }
}
