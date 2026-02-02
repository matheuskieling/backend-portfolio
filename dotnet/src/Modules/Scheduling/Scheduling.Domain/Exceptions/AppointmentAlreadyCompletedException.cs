using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class AppointmentAlreadyCompletedException : DomainException
{
    private const string ErrorCode = "APPOINTMENT_ALREADY_COMPLETED";

    public AppointmentAlreadyCompletedException(Guid appointmentId)
        : base(ErrorCode, $"Appointment '{appointmentId}' has already been completed")
    {
    }
}
