namespace Identity.Application.DTOs;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    IReadOnlyList<string> Permissions);
