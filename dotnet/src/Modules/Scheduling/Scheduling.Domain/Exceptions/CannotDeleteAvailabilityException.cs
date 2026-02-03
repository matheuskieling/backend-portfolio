using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class CannotDeleteAvailabilityException : ValidationException
{
    private const string ErrorCode = "CANNOT_DELETE_AVAILABILITY";

    public CannotDeleteAvailabilityException()
        : base(ErrorCode, "Cannot delete availability because it has booked time slots")
    {
    }
}
