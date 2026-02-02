using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class CancellationDeadlinePassedException : DomainException
{
    private const string ErrorCode = "CANCELLATION_DEADLINE_PASSED";

    public CancellationDeadlinePassedException(int deadlineMinutes)
        : base(ErrorCode, $"Cannot cancel appointment. Cancellation deadline is {deadlineMinutes} minutes before the appointment")
    {
    }
}
