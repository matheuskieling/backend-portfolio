using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class ScheduleNotFoundException : DomainException
{
    private const string ErrorCode = "SCHEDULE_NOT_FOUND";

    public ScheduleNotFoundException(Guid scheduleId)
        : base(ErrorCode, $"Schedule with ID '{scheduleId}' was not found")
    {
    }
}
