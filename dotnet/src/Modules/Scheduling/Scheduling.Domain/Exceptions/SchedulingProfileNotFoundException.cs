using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class SchedulingProfileNotFoundException : NotFoundException
{
    private const string ErrorCode = "SCHEDULING_PROFILE_NOT_FOUND";

    public SchedulingProfileNotFoundException(Guid profileId)
        : base(ErrorCode, $"Scheduling profile with ID '{profileId}' was not found")
    {
    }
}
