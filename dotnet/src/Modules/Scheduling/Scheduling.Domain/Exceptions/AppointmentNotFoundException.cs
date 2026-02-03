using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class AppointmentNotFoundException : NotFoundException
{
    private const string ErrorCode = "APPOINTMENT_NOT_FOUND";

    public AppointmentNotFoundException(Guid appointmentId)
        : base(ErrorCode, $"Appointment with ID '{appointmentId}' was not found")
    {
    }
}
