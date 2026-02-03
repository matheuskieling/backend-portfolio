using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class CannotBlockBookedSlotException : ValidationException
{
    private const string ErrorCode = "CANNOT_BLOCK_BOOKED_SLOT";

    public CannotBlockBookedSlotException(Guid timeSlotId)
        : base(ErrorCode, $"Cannot block time slot '{timeSlotId}' because it is already booked")
    {
    }
}
