using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class TimeSlotAlreadyBlockedException : ValidationException
{
    private const string ErrorCode = "TIME_SLOT_ALREADY_BLOCKED";

    public TimeSlotAlreadyBlockedException(Guid timeSlotId)
        : base(ErrorCode, $"Time slot '{timeSlotId}' is already blocked")
    {
    }
}
