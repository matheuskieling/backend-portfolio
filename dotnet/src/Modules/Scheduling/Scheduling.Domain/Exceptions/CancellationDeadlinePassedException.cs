using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class CancellationDeadlinePassedException : ValidationException
{
    private const string ErrorCode = "CANCELLATION_DEADLINE_PASSED";

    public CancellationDeadlinePassedException(int deadlineMinutes)
        : base(ErrorCode, $"Cannot cancel appointment. Cancellation deadline is {deadlineMinutes} minutes before the appointment")
    {
    }
}
