using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class AvailabilityNotFoundException : NotFoundException
{
    private const string ErrorCode = "AVAILABILITY_NOT_FOUND";

    public AvailabilityNotFoundException(Guid availabilityId)
        : base(ErrorCode, $"Availability with ID '{availabilityId}' was not found")
    {
    }
}
