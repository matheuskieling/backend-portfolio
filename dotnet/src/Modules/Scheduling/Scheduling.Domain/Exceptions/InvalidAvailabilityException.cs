using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class InvalidAvailabilityException : ValidationException
{
    private const string ErrorCode = "INVALID_AVAILABILITY";

    public InvalidAvailabilityException(string message)
        : base(ErrorCode, message)
    {
    }
}
