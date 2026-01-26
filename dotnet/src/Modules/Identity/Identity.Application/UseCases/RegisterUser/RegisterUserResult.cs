namespace Identity.Application.UseCases.RegisterUser;

public sealed record RegisterUserResult(
    Guid UserId,
    string Email,
    string FullName);
