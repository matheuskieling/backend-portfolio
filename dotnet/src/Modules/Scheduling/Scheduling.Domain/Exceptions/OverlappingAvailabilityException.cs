using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class OverlappingAvailabilityException : DomainException
{
    private const string ErrorCode = "OVERLAPPING_AVAILABILITY";

    public OverlappingAvailabilityException(DateTimeOffset startTime, DateTimeOffset endTime)
        : base(ErrorCode, $"An availability already exists that overlaps with the time range {startTime:u} to {endTime:u}")
    {
    }
}
