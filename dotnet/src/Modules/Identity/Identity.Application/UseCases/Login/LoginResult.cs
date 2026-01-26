namespace Identity.Application.UseCases.Login;

public sealed record LoginResult(
    string Token,
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles);
