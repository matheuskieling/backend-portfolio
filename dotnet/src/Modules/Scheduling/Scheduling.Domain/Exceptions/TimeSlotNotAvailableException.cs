using Common.Domain;
using Scheduling.Domain.Enums;

namespace Scheduling.Domain.Exceptions;

public sealed class TimeSlotNotAvailableException : DomainException
{
    private const string ErrorCode = "TIME_SLOT_NOT_AVAILABLE";

    public TimeSlotNotAvailableException(Guid timeSlotId, TimeSlotStatus currentStatus)
        : base(ErrorCode, $"Time slot '{timeSlotId}' is not available for booking. Current status: {currentStatus}")
    {
    }
}
