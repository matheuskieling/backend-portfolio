namespace Common.Contracts.Identity;

public record UserBasicInfoDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName
);
