using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class UnauthorizedSchedulingAccessException : ForbiddenException
{
    private const string ErrorCode = "UNAUTHORIZED_SCHEDULING_ACCESS";

    public UnauthorizedSchedulingAccessException(string message)
        : base(ErrorCode, message)
    {
    }

    public static UnauthorizedSchedulingAccessException NotProfileOwner() =>
        new("You do not have permission to access this profile");

    public static UnauthorizedSchedulingAccessException NotAppointmentParticipant() =>
        new("You are not a participant of this appointment");
}
