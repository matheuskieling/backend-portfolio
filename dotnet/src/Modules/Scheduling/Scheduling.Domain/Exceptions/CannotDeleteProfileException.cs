using Common.Domain.Exceptions;

namespace Scheduling.Domain.Exceptions;

public sealed class CannotDeleteProfileException : ValidationException
{
    private const string ErrorCode = "CANNOT_DELETE_PROFILE";

    public CannotDeleteProfileException()
        : base(ErrorCode, "Cannot delete profile because it has existing appointments")
    {
    }
}
