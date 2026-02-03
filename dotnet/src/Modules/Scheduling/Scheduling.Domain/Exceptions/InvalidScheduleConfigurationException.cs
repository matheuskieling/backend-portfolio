using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class InvalidScheduleConfigurationException : ValidationException
{
    private const string ErrorCode = "INVALID_SCHEDULE_CONFIGURATION";

    public InvalidScheduleConfigurationException(string message)
        : base(ErrorCode, message)
    {
    }
}
