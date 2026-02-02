using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class InvalidScheduleConfigurationException : DomainException
{
    private const string ErrorCode = "INVALID_SCHEDULE_CONFIGURATION";

    public InvalidScheduleConfigurationException(string message)
        : base(ErrorCode, message)
    {
    }
}
