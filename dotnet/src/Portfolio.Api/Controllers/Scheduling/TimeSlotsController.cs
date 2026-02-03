using Common.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.Scheduling;
using Scheduling.Application.UseCases.TimeSlots;

namespace Portfolio.Api.Controllers.Scheduling;

/// <summary>
/// Manages time slots for scheduling profiles.
/// </summary>
[ApiController]
[Route("api/scheduling/profiles/{profileId:guid}/slots")]
[Tags("Scheduling - Time Slots")]
[Produces("application/json")]
public class TimeSlotsController : ControllerBase
{
    private readonly GetAvailableSlotsHandler _getAvailableSlotsHandler;
    private readonly BlockSlotsHandler _blockSlotsHandler;
    private readonly UnblockSlotsHandler _unblockSlotsHandler;

    public TimeSlotsController(
        GetAvailableSlotsHandler getAvailableSlotsHandler,
        BlockSlotsHandler blockSlotsHandler,
        UnblockSlotsHandler unblockSlotsHandler)
    {
        _getAvailableSlotsHandler = getAvailableSlotsHandler;
        _blockSlotsHandler = blockSlotsHandler;
        _unblockSlotsHandler = unblockSlotsHandler;
    }

    /// <summary>
    /// Gets available time slots for a profile within a date range (public).
    /// </summary>
    /// <param name="profileId">The profile ID to get available slots for.</param>
    /// <param name="from">Start of date range.</param>
    /// <param name="to">End of date range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of available time slots.</returns>
    /// <response code="200">Available slots retrieved successfully.</response>
    /// <response code="404">Profile not found.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AvailableSlotResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AvailableSlotResponse>>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<IReadOnlyList<AvailableSlotResponse>>> GetAvailableSlots(
        Guid profileId,
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var query = new GetAvailableSlotsQuery(profileId, from, to);
        var result = await _getAvailableSlotsHandler.HandleAsync(query, cancellationToken);

        var slots = result.Select(s => new AvailableSlotResponse(
            s.Id,
            s.AvailabilityId,
            s.StartTime,
            s.EndTime)).ToList();

        return ApiResponse.Success<IReadOnlyList<AvailableSlotResponse>>(slots);
    }

    /// <summary>
    /// Blocks multiple time slots (owner only).
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="request">The slot IDs to block.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The block operation result.</returns>
    /// <response code="200">Slots successfully blocked.</response>
    /// <response code="400">Invalid request or slots cannot be blocked (e.g., already booked).</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to block slots for this profile.</response>
    /// <response code="404">Profile not found.</response>
    [HttpPost("block")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<BlockUnblockResponse>> BlockSlots(
        Guid profileId,
        [FromBody] BlockSlotsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BlockSlotsCommand(profileId, request.SlotIds);
        var result = await _blockSlotsHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Success(new BlockUnblockResponse(
            result.BlockedCount,
            result.BlockedSlotIds));
    }

    /// <summary>
    /// Unblocks multiple time slots (owner only).
    /// </summary>
    /// <param name="profileId">The profile ID.</param>
    /// <param name="request">The slot IDs to unblock.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unblock operation result.</returns>
    /// <response code="200">Slots successfully unblocked.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to unblock slots for this profile.</response>
    /// <response code="404">Profile not found.</response>
    [HttpPost("unblock")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<BlockUnblockResponse>> UnblockSlots(
        Guid profileId,
        [FromBody] UnblockSlotsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UnblockSlotsCommand(profileId, request.SlotIds);
        var result = await _unblockSlotsHandler.HandleAsync(command, cancellationToken);

        return ApiResponse.Success(new BlockUnblockResponse(
            result.UnblockedCount,
            result.UnblockedSlotIds));
    }
}
