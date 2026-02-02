using Common.Domain;

namespace Scheduling.Domain.Exceptions;

public sealed class SchedulingProfileAlreadyExistsException : DomainException
{
    private const string ErrorCode = "SCHEDULING_PROFILE_ALREADY_EXISTS";

    public SchedulingProfileAlreadyExistsException(string message)
        : base(ErrorCode, message)
    {
    }

    public static SchedulingProfileAlreadyExistsException IndividualProfileExists(Guid externalUserId) =>
        new($"User '{externalUserId}' already has an individual profile");

    public static SchedulingProfileAlreadyExistsException BusinessNameExists(string businessName) =>
        new($"A business profile with name '{businessName}' already exists for this user");
}
