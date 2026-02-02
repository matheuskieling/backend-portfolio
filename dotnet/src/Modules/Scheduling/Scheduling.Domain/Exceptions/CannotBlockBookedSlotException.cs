using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class CannotBlockBookedSlotException : DomainException
{
    private const string ErrorCode = "CANNOT_BLOCK_BOOKED_SLOT";

    public CannotBlockBookedSlotException(Guid timeSlotId)
        : base(ErrorCode, $"Cannot block time slot '{timeSlotId}' because it is already booked")
    {
    }
}
