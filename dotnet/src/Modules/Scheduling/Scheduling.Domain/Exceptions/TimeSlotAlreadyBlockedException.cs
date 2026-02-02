using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class TimeSlotAlreadyBlockedException : DomainException
{
    private const string ErrorCode = "TIME_SLOT_ALREADY_BLOCKED";

    public TimeSlotAlreadyBlockedException(Guid timeSlotId)
        : base(ErrorCode, $"Time slot '{timeSlotId}' is already blocked")
    {
    }
}
