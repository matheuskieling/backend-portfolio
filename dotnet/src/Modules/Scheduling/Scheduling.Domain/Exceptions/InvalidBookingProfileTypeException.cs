using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class InvalidBookingProfileTypeException : ValidationException
{
    private const string ErrorCode = "INVALID_BOOKING_PROFILE_TYPE";

    private InvalidBookingProfileTypeException(string message)
        : base(ErrorCode, message)
    {
    }

    public static InvalidBookingProfileTypeException HostMustBeBusiness()
        => new("Only business profiles can receive appointments");

    public static InvalidBookingProfileTypeException GuestMustBeIndividual()
        => new("Only individual profiles can book appointments");
}
