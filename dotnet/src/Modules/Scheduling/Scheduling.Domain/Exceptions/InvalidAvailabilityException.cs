using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class InvalidAvailabilityException : DomainException
{
    private const string ErrorCode = "INVALID_AVAILABILITY";

    public InvalidAvailabilityException(string message)
        : base(ErrorCode, message)
    {
    }
}
