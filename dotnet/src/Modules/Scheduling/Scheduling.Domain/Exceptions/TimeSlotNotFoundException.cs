using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class TimeSlotNotFoundException : NotFoundException
{
    private const string ErrorCode = "TIME_SLOT_NOT_FOUND";

    public TimeSlotNotFoundException(Guid timeSlotId)
        : base(ErrorCode, $"Time slot with ID '{timeSlotId}' was not found")
    {
    }
}
