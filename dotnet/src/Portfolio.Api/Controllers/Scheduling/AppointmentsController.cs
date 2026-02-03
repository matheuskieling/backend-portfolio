using Common.Contracts;
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
    /// <param name="profileId">The host profile ID to book with.</param>
    /// <param name="request">The appointment booking details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created appointment.</returns>
    /// <response code="201">Appointment successfully booked.</response>
    /// <response code="400">Invalid request or domain validation error (e.g., booking window violation).</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to book with this profile or guest profile not owned by user.</response>
    /// <response code="404">Host profile or time slot not found.</response>
    /// <response code="409">Time slot is not available (already booked or blocked).</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentResponse>), StatusCodes.Status409Conflict)]
    public async Task<ApiResponse<AppointmentResponse>> BookAppointment(
        Guid profileId,
        [FromBody] BookAppointmentRequest request,
        CancellationToken cancellationToken)
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

    /// <summary>
    /// Lists appointments for a profile (as host or guest).
    /// </summary>
    /// <param name="profileId">The profile ID to list appointments for.</param>
    /// <param name="status">Optional filter by appointment status (Pending, Confirmed, Completed, Canceled).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of appointments for the profile.</returns>
    /// <response code="200">Appointments retrieved successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to view appointments for this profile.</response>
    /// <response code="404">Profile not found.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentResponse>>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AppointmentResponse>>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<IReadOnlyList<AppointmentResponse>>> GetAppointments(
        Guid profileId,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        AppointmentStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<AppointmentStatus>(status, true, out var parsed))
                throw new ArgumentException($"Invalid status: {status}");
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

    /// <summary>
    /// Gets an appointment by ID.
    /// </summary>
    /// <param name="profileId">The profile ID associated with the appointment.</param>
    /// <param name="appointmentId">The appointment ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The appointment details.</returns>
    /// <response code="200">Appointment retrieved successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to view this appointment.</response>
    /// <response code="404">Profile or appointment not found.</response>
    [HttpGet("{appointmentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailResponse>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<AppointmentDetailResponse>), StatusCodes.Status404NotFound)]
    public async Task<ApiResponse<AppointmentDetailResponse>> GetAppointmentById(
        Guid profileId,
        Guid appointmentId,
        CancellationToken cancellationToken)
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

    /// <summary>
    /// Cancels an appointment (either host or guest can cancel).
    /// </summary>
    /// <param name="profileId">The profile ID associated with the appointment.</param>
    /// <param name="appointmentId">The appointment ID to cancel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Appointment successfully canceled.</response>
    /// <response code="400">Cannot cancel appointment (e.g., already completed or past cancellation deadline).</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to cancel this appointment.</response>
    /// <response code="404">Profile or appointment not found.</response>
    [HttpPost("{appointmentId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAppointment(
        Guid profileId,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var command = new CancelAppointmentCommand(profileId, appointmentId);
        await _cancelAppointmentHandler.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Marks an appointment as completed (host only).
    /// </summary>
    /// <param name="profileId">The host profile ID.</param>
    /// <param name="appointmentId">The appointment ID to complete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Appointment successfully marked as completed.</response>
    /// <response code="400">Cannot complete appointment (e.g., already canceled or not yet due).</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Not authorized to complete this appointment (host only).</response>
    /// <response code="404">Profile or appointment not found.</response>
    [HttpPost("{appointmentId:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteAppointment(
        Guid profileId,
        Guid appointmentId,
        CancellationToken cancellationToken)
    {
        var command = new CompleteAppointmentCommand(profileId, appointmentId);
        await _completeAppointmentHandler.HandleAsync(command, cancellationToken);
        return NoContent();
    }
}
