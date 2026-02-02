using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class SelfBookingNotAllowedException : DomainException
{
    private const string ErrorCode = "SELF_BOOKING_NOT_ALLOWED";

    public SelfBookingNotAllowedException()
        : base(ErrorCode, "You cannot book an appointment with your own profile")
    {
    }
}
