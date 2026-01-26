using Identity.Domain.Enums;

namespace Identity.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    UserStatus Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
