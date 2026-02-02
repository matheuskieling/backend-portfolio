using Common.Contracts;
using Common.Domain;
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
        try
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
        catch (DomainException ex)
        {
            return ApiResponse.NotFound<IReadOnlyList<AvailableSlotResponse>>(ex.Message);
        }
    }

    /// <summary>
    /// Blocks multiple time slots (owner only).
    /// </summary>
    [HttpPost("block")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<BlockUnblockResponse>> BlockSlots(
        Guid profileId,
        [FromBody] BlockSlotsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new BlockSlotsCommand(profileId, request.SlotIds);
            var result = await _blockSlotsHandler.HandleAsync(command, cancellationToken);

            return ApiResponse.Success(new BlockUnblockResponse(
                result.BlockedCount,
                result.BlockedSlotIds));
        }
        catch (DomainException ex)
        {
            return HandleDomainException<BlockUnblockResponse>(ex);
        }
    }

    /// <summary>
    /// Unblocks multiple time slots (owner only).
    /// </summary>
    [HttpPost("unblock")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<BlockUnblockResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<BlockUnblockResponse>> UnblockSlots(
        Guid profileId,
        [FromBody] UnblockSlotsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UnblockSlotsCommand(profileId, request.SlotIds);
            var result = await _unblockSlotsHandler.HandleAsync(command, cancellationToken);

            return ApiResponse.Success(new BlockUnblockResponse(
                result.UnblockedCount,
                result.UnblockedSlotIds));
        }
        catch (DomainException ex)
        {
            return HandleDomainException<BlockUnblockResponse>(ex);
        }
    }

    private static ApiResponse<T> HandleDomainException<T>(DomainException ex)
    {
        return ex.Code switch
        {
            "SCHEDULING_PROFILE_NOT_FOUND" => ApiResponse.NotFound<T>(ex.Message),
            "UNAUTHORIZED_SCHEDULING_ACCESS" => ApiResponse.Forbidden<T>(ex.Message),
            _ => ApiResponse.Failure<T>(ex.Code, ex.Message)
        };
    }
}
