using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class BookingWindowViolationException : ValidationException
{
    private const string ErrorCode = "BOOKING_WINDOW_VIOLATION";

    private BookingWindowViolationException(string message)
        : base(ErrorCode, message)
    {
    }

    public static BookingWindowViolationException TooSoon(int minAdvanceMinutes) =>
        new($"Booking must be made at least {minAdvanceMinutes} minutes in advance");

    public static BookingWindowViolationException TooFarInFuture(int maxAdvanceDays) =>
        new($"Booking cannot be made more than {maxAdvanceDays} days in advance");
}
