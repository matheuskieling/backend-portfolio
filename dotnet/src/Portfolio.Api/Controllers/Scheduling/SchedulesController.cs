using Common.Contracts;
using Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.Scheduling;
using Scheduling.Application.UseCases.Schedules;

namespace Portfolio.Api.Controllers.Scheduling;

/// <summary>
/// Manages recurring schedules for scheduling profiles.
/// </summary>
[ApiController]
[Route("api/scheduling/profiles/{profileId:guid}/schedules")]
[Tags("Scheduling - Schedules")]
[Produces("application/json")]
[Authorize]
public class SchedulesController : ControllerBase
{
    private readonly CreateScheduleHandler _createScheduleHandler;
    private readonly GetSchedulesHandler _getSchedulesHandler;
    private readonly GetScheduleByIdHandler _getScheduleByIdHandler;
    private readonly UpdateScheduleHandler _updateScheduleHandler;
    private readonly DeleteScheduleHandler _deleteScheduleHandler;
    private readonly PauseScheduleHandler _pauseScheduleHandler;
    private readonly ResumeScheduleHandler _resumeScheduleHandler;
    private readonly GenerateAvailabilitiesHandler _generateAvailabilitiesHandler;

    public SchedulesController(
        CreateScheduleHandler createScheduleHandler,
        GetSchedulesHandler getSchedulesHandler,
        GetScheduleByIdHandler getScheduleByIdHandler,
        UpdateScheduleHandler updateScheduleHandler,
        DeleteScheduleHandler deleteScheduleHandler,
        PauseScheduleHandler pauseScheduleHandler,
        ResumeScheduleHandler resumeScheduleHandler,
        GenerateAvailabilitiesHandler generateAvailabilitiesHandler)
    {
        _createScheduleHandler = createScheduleHandler;
        _getSchedulesHandler = getSchedulesHandler;
        _getScheduleByIdHandler = getScheduleByIdHandler;
        _updateScheduleHandler = updateScheduleHandler;
        _deleteScheduleHandler = deleteScheduleHandler;
        _pauseScheduleHandler = pauseScheduleHandler;
        _resumeScheduleHandler = resumeScheduleHandler;
        _generateAvailabilitiesHandler = generateAvailabilitiesHandler;
    }

    /// <summary>
    /// Creates a new recurring schedule for the profile.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ScheduleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<ScheduleResponse>> CreateSchedule(
        Guid profileId,
        [FromBody] CreateScheduleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TimeOnly.TryParse(request.StartTimeOfDay, out var startTime))
                return ApiResponse.Failure<ScheduleResponse>("INVALID_TIME", "Invalid start time format. Use HH:mm.");

            if (!TimeOnly.TryParse(request.EndTimeOfDay, out var endTime))
                return ApiResponse.Failure<ScheduleResponse>("INVALID_TIME", "Invalid end time format. Use HH:mm.");

            if (!DateOnly.TryParse(request.EffectiveFrom, out var effectiveFrom))
                return ApiResponse.Failure<ScheduleResponse>("INVALID_DATE", "Invalid effective from date. Use yyyy-MM-dd.");

            DateOnly? effectiveUntil = null;
            if (!string.IsNullOrEmpty(request.EffectiveUntil))
            {
                if (!DateOnly.TryParse(request.EffectiveUntil, out var until))
                    return ApiResponse.Failure<ScheduleResponse>("INVALID_DATE", "Invalid effective until date. Use yyyy-MM-dd.");
                effectiveUntil = until;
            }

            var daysOfWeek = request.DaysOfWeek.Select(d => (DayOfWeek)d).ToArray();

            var command = new CreateScheduleCommand(
                profileId,
                request.Name,
                daysOfWeek,
                startTime,
                endTime,
                request.SlotDurationMinutes,
                effectiveFrom,
                effectiveUntil,
                request.MinAdvanceBookingMinutes,
                request.MaxAdvanceBookingDays,
                request.CancellationDeadlineMinutes);

            var result = await _createScheduleHandler.HandleAsync(command, cancellationToken);

            return ApiResponse.Created(new ScheduleResponse(
                result.Id,
                result.Name,
                result.DaysOfWeek.Select(d => (int)d).ToArray(),
                result.StartTimeOfDay.ToString("HH:mm"),
                result.EndTimeOfDay.ToString("HH:mm"),
                result.SlotDurationMinutes,
                result.EffectiveFrom.ToString("yyyy-MM-dd"),
                result.EffectiveUntil?.ToString("yyyy-MM-dd"),
                result.IsActive,
                DateTime.UtcNow));
        }
        catch (DomainException ex)
        {
            return HandleDomainException<ScheduleResponse>(ex);
        }
    }

    /// <summary>
    /// Lists all schedules for the profile.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ScheduleResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ScheduleResponse>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ScheduleResponse>>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<IReadOnlyList<ScheduleResponse>>> GetSchedules(
        Guid profileId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetSchedulesQuery(profileId);
            var result = await _getSchedulesHandler.HandleAsync(query, cancellationToken);

            var schedules = result.Select(s => new ScheduleResponse(
                s.Id,
                s.Name,
                s.DaysOfWeek.Select(d => (int)d).ToArray(),
                s.StartTimeOfDay.ToString("HH:mm"),
                s.EndTimeOfDay.ToString("HH:mm"),
                s.SlotDurationMinutes,
                s.EffectiveFrom.ToString("yyyy-MM-dd"),
                s.EffectiveUntil?.ToString("yyyy-MM-dd"),
                s.IsActive,
                s.CreatedAt)).ToList();

            return ApiResponse.Success<IReadOnlyList<ScheduleResponse>>(schedules);
        }
        catch (DomainException ex)
        {
            return HandleDomainException<IReadOnlyList<ScheduleResponse>>(ex);
        }
    }

    /// <summary>
    /// Gets a schedule by ID.
    /// </summary>
    [HttpGet("{scheduleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ScheduleDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleDetailResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<ScheduleDetailResponse>> GetScheduleById(
        Guid profileId,
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetScheduleByIdQuery(profileId, scheduleId);
            var result = await _getScheduleByIdHandler.HandleAsync(query, cancellationToken);

            return ApiResponse.Success(new ScheduleDetailResponse(
                result.Id,
                result.Name,
                result.DaysOfWeek.Select(d => (int)d).ToArray(),
                result.StartTimeOfDay.ToString("HH:mm"),
                result.EndTimeOfDay.ToString("HH:mm"),
                result.SlotDurationMinutes,
                result.MinAdvanceBookingMinutes,
                result.MaxAdvanceBookingDays,
                result.CancellationDeadlineMinutes,
                result.EffectiveFrom.ToString("yyyy-MM-dd"),
                result.EffectiveUntil?.ToString("yyyy-MM-dd"),
                result.IsActive,
                result.CreatedAt,
                result.UpdatedAt));
        }
        catch (DomainException ex)
        {
            return HandleDomainException<ScheduleDetailResponse>(ex);
        }
    }

    /// <summary>
    /// Updates a schedule.
    /// </summary>
    [HttpPut("{scheduleId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<ScheduleResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<ScheduleResponse>> UpdateSchedule(
        Guid profileId,
        Guid scheduleId,
        [FromBody] UpdateScheduleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!TimeOnly.TryParse(request.StartTimeOfDay, out var startTime))
                return ApiResponse.Failure<ScheduleResponse>("INVALID_TIME", "Invalid start time format. Use HH:mm.");

            if (!TimeOnly.TryParse(request.EndTimeOfDay, out var endTime))
                return ApiResponse.Failure<ScheduleResponse>("INVALID_TIME", "Invalid end time format. Use HH:mm.");

            if (!DateOnly.TryParse(request.EffectiveFrom, out var effectiveFrom))
                return ApiResponse.Failure<ScheduleResponse>("INVALID_DATE", "Invalid effective from date. Use yyyy-MM-dd.");

            DateOnly? effectiveUntil = null;
            if (!string.IsNullOrEmpty(request.EffectiveUntil))
            {
                if (!DateOnly.TryParse(request.EffectiveUntil, out var until))
                    return ApiResponse.Failure<ScheduleResponse>("INVALID_DATE", "Invalid effective until date. Use yyyy-MM-dd.");
                effectiveUntil = until;
            }

            var daysOfWeek = request.DaysOfWeek.Select(d => (DayOfWeek)d).ToArray();

            var command = new UpdateScheduleCommand(
                profileId,
                scheduleId,
                request.Name,
                daysOfWeek,
                startTime,
                endTime,
                request.SlotDurationMinutes,
                effectiveFrom,
                effectiveUntil,
                request.MinAdvanceBookingMinutes,
                request.MaxAdvanceBookingDays,
                request.CancellationDeadlineMinutes);

            var result = await _updateScheduleHandler.HandleAsync(command, cancellationToken);

            return ApiResponse.Success(new ScheduleResponse(
                result.Id,
                result.Name,
                result.DaysOfWeek.Select(d => (int)d).ToArray(),
                result.StartTimeOfDay.ToString("HH:mm"),
                result.EndTimeOfDay.ToString("HH:mm"),
                result.SlotDurationMinutes,
                result.EffectiveFrom.ToString("yyyy-MM-dd"),
                result.EffectiveUntil?.ToString("yyyy-MM-dd"),
                result.IsActive,
                DateTime.UtcNow));
        }
        catch (DomainException ex)
        {
            return HandleDomainException<ScheduleResponse>(ex);
        }
    }

    /// <summary>
    /// Deletes a schedule.
    /// </summary>
    [HttpDelete("{scheduleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSchedule(
        Guid profileId,
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeleteScheduleCommand(profileId, scheduleId);
            await _deleteScheduleHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return HandleDomainExceptionResult(ex);
        }
    }

    /// <summary>
    /// Pauses a schedule (stops generating new availabilities).
    /// </summary>
    [HttpPost("{scheduleId:guid}/pause")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PauseSchedule(
        Guid profileId,
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new PauseScheduleCommand(profileId, scheduleId);
            await _pauseScheduleHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return HandleDomainExceptionResult(ex);
        }
    }

    /// <summary>
    /// Resumes a paused schedule.
    /// </summary>
    [HttpPost("{scheduleId:guid}/resume")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResumeSchedule(
        Guid profileId,
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new ResumeScheduleCommand(profileId, scheduleId);
            await _resumeScheduleHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return HandleDomainExceptionResult(ex);
        }
    }

    /// <summary>
    /// Generates availabilities from the schedule for a date range.
    /// </summary>
    [HttpPost("{scheduleId:guid}/generate")]
    [ProducesResponseType(typeof(ApiResponse<GenerateAvailabilitiesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GenerateAvailabilitiesResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<GenerateAvailabilitiesResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<GenerateAvailabilitiesResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<GenerateAvailabilitiesResponse>> GenerateAvailabilities(
        Guid profileId,
        Guid scheduleId,
        [FromBody] GenerateAvailabilitiesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!DateOnly.TryParse(request.FromDate, out var fromDate))
                return ApiResponse.Failure<GenerateAvailabilitiesResponse>("INVALID_DATE", "Invalid from date. Use yyyy-MM-dd.");

            if (!DateOnly.TryParse(request.ToDate, out var toDate))
                return ApiResponse.Failure<GenerateAvailabilitiesResponse>("INVALID_DATE", "Invalid to date. Use yyyy-MM-dd.");

            var command = new GenerateAvailabilitiesCommand(profileId, scheduleId, fromDate, toDate);
            var result = await _generateAvailabilitiesHandler.HandleAsync(command, cancellationToken);

            return ApiResponse.Success(new GenerateAvailabilitiesResponse(
                result.GeneratedCount,
                result.SkippedCount,
                result.Availabilities.Select(a => new GeneratedAvailabilityInfo(
                    a.Id,
                    a.StartTime,
                    a.EndTime,
                    a.SlotCount)).ToList()));
        }
        catch (DomainException ex)
        {
            return HandleDomainException<GenerateAvailabilitiesResponse>(ex);
        }
    }

    private static ApiResponse<T> HandleDomainException<T>(DomainException ex)
    {
        return ex.Code switch
        {
            "SCHEDULING_PROFILE_NOT_FOUND" or "SCHEDULE_NOT_FOUND" => ApiResponse.NotFound<T>(ex.Message),
            "UNAUTHORIZED_SCHEDULING_ACCESS" => ApiResponse.Forbidden<T>(ex.Message),
            _ => ApiResponse.Failure<T>(ex.Code, ex.Message)
        };
    }

    private static IActionResult HandleDomainExceptionResult(DomainException ex)
    {
        return ex.Code switch
        {
            "SCHEDULING_PROFILE_NOT_FOUND" or "SCHEDULE_NOT_FOUND" => ApiResponse.NotFound<object>(ex.Message),
            "UNAUTHORIZED_SCHEDULING_ACCESS" => ApiResponse.Forbidden<object>(ex.Message),
            _ => ApiResponse.Failure<object>(ex.Code, ex.Message)
        };
    }
}
