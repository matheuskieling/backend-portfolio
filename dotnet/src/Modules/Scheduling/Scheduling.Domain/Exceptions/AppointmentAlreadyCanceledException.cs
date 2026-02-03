using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class AppointmentAlreadyCanceledException : ValidationException
{
    private const string ErrorCode = "APPOINTMENT_ALREADY_CANCELED";

    public AppointmentAlreadyCanceledException(Guid appointmentId)
        : base(ErrorCode, $"Appointment '{appointmentId}' has already been canceled")
    {
    }
}
