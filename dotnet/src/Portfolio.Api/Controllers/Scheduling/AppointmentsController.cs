using Common.Contracts;
using Common.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Contracts.Scheduling;
using Scheduling.Application.UseCases.Appointments;
using Scheduling.Domain.Enums;

namespace Portfolio.Api.Controllers.Scheduling;

/// <summary>
/// Manages appointments for scheduling profiles.
/// </summary>
[ApiController]
[Route("api/scheduling/profiles/{profileId:guid}/appointments")]
[Tags("Scheduling - Appointments")]
[Produces("application/json")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly BookAppointmentHandler _bookAppointmentHandler;
    private readonly GetAppointmentsHandler _getAppointmentsHandler;
    private readonly GetAppointmentByIdHandler _getAppointmentByIdHandler;
    private readonly CancelAppointmentHandler _cancelAppointmentHandler;
    private readonly CompleteAppointmentHandler _completeAppointmentHandler;

    public AppointmentsController(
        BookAppointmentHandler bookAppointmentHandler,
        GetAppointmentsHandler getAppointmentsHandler,
        GetAppointmentByIdHandler getAppointmentByIdHandler,
        CancelAppointmentHandler cancelAppointmentHandler,
        CompleteAppointmentHandler completeAppointmentHandler)
    {
        _bookAppointmentHandler = bookAppointmentHandler;
        _getAppointmentsHandler = getAppointmentsHandler;
        _getAppointmentByIdHandler = getAppointmentByIdHandler;
        _cancelAppointmentHandler = cancelAppointmentHandler;
        _completeAppointmentHandler = completeAppointmentHandler;
    }

    /// <summary>
    /// Books an appointment with a host profile.
    /// The guest profile must be owned by the current user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status409Conflict)]
    public async Task<ApiResponse<AppointmentResponse>> BookAppointment(
        Guid profileId,
        [FromBody] BookAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new BookAppointmentCommand(
                profileId,
                request.GuestProfileId,
                request.TimeSlotId);

            var result = await _bookAppointmentHandler.HandleAsync(command, cancellationToken);

            return ApiResponse.Created(new AppointmentResponse(
                result.Id,
                result.TimeSlotId,
                result.HostProfileId,
                result.GuestProfileId,
                result.StartTime,
                result.EndTime,
                result.Status.ToString(),
                false,
                result.CreatedAt,
                null,
                null));
        }
        catch (DomainException ex)
        {
            return HandleDomainException<AppointmentResponse>(ex);
        }
    }

    /// <summary>
    /// Lists appointments for a profile (as host or guest).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentResponse>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentResponse>>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<IReadOnlyList<AppointmentResponse>>> GetAppointments(
        Guid profileId,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            AppointmentStatus? statusFilter = null;
            if (!string.IsNullOrEmpty(status))
            {
                if (!Enum.TryParse<AppointmentStatus>(status, true, out var parsed))
                    return ApiResponse.Failure<IReadOnlyList<AppointmentResponse>>("INVALID_STATUS", $"Invalid status: {status}");
                statusFilter = parsed;
            }

            var query = new GetAppointmentsQuery(profileId, statusFilter);
            var result = await _getAppointmentsHandler.HandleAsync(query, cancellationToken);

            var appointments = result.Select(a => new AppointmentResponse(
                a.Id,
                a.TimeSlotId,
                a.HostProfileId,
                a.GuestProfileId,
                a.StartTime,
                a.EndTime,
                a.Status.ToString(),
                a.IsHost,
                a.CreatedAt,
                a.CanceledAt,
                a.CompletedAt)).ToList();

            return ApiResponse.Success<IReadOnlyList<AppointmentResponse>>(appointments);
        }
        catch (DomainException ex)
        {
            return HandleDomainException<IReadOnlyList<AppointmentResponse>>(ex);
        }
    }

    /// <summary>
    /// Gets an appointment by ID.
    /// </summary>
    [HttpGet("{appointmentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<AppointmentDetailResponse>> GetAppointmentById(
        Guid profileId,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetAppointmentByIdQuery(profileId, appointmentId);
            var result = await _getAppointmentByIdHandler.HandleAsync(query, cancellationToken);

            return ApiResponse.Success(new AppointmentDetailResponse(
                result.Id,
                result.TimeSlotId,
                result.HostProfileId,
                result.GuestProfileId,
                result.StartTime,
                result.EndTime,
                result.Status.ToString(),
                result.IsHost,
                result.CreatedAt,
                result.CanceledAt,
                result.CanceledBy,
                result.CompletedAt));
        }
        catch (DomainException ex)
        {
            return HandleDomainException<AppointmentDetailResponse>(ex);
        }
    }

    /// <summary>
    /// Cancels an appointment (either host or guest can cancel).
    /// </summary>
    [HttpPost("{appointmentId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAppointment(
        Guid profileId,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CancelAppointmentCommand(profileId, appointmentId);
            await _cancelAppointmentHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return ex.Code switch
            {
                "SCHEDULING_PROFILE_NOT_FOUND" or "APPOINTMENT_NOT_FOUND" or "AVAILABILITY_NOT_FOUND" => ApiResponse.NotFound<object>(ex.Message),
                "UNAUTHORIZED_SCHEDULING_ACCESS" => ApiResponse.Forbidden<object>(ex.Message),
                _ => ApiResponse.Failure<object>(ex.Code, ex.Message)
            };
        }
    }

    /// <summary>
    /// Marks an appointment as completed (host only).
    /// </summary>
    [HttpPost("{appointmentId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteAppointment(
        Guid profileId,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CompleteAppointmentCommand(profileId, appointmentId);
            await _completeAppointmentHandler.HandleAsync(command, cancellationToken);
            return NoContent();
        }
        catch (DomainException ex)
        {
            return ex.Code switch
            {
                "SCHEDULING_PROFILE_NOT_FOUND" or "APPOINTMENT_NOT_FOUND" => ApiResponse.NotFound<object>(ex.Message),
                "UNAUTHORIZED_SCHEDULING_ACCESS" => ApiResponse.Forbidden<object>(ex.Message),
                _ => ApiResponse.Failure<object>(ex.Code, ex.Message)
            };
        }
    }

    private static ApiResponse<T> HandleDomainException<T>(DomainException ex)
    {
        return ex.Code switch
        {
            "SCHEDULING_PROFILE_NOT_FOUND" or "APPOINTMENT_NOT_FOUND" or "TIME_SLOT_NOT_FOUND" => ApiResponse.NotFound<T>(ex.Message),
            "UNAUTHORIZED_SCHEDULING_ACCESS" => ApiResponse.Forbidden<T>(ex.Message),
            "SELF_BOOKING_NOT_ALLOWED" or "TIME_SLOT_NOT_AVAILABLE" or "BOOKING_WINDOW_VIOLATION" => ApiResponse.Failure<T>(ex.Code, ex.Message),
            _ => ApiResponse.Failure<T>(ex.Code, ex.Message)
        };
    }
}
