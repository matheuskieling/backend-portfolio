using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class AppointmentAlreadyCanceledException : DomainException
{
    private const string ErrorCode = "APPOINTMENT_ALREADY_CANCELED";

    public AppointmentAlreadyCanceledException(Guid appointmentId)
        : base(ErrorCode, $"Appointment '{appointmentId}' has already been canceled")
    {
    }
}
