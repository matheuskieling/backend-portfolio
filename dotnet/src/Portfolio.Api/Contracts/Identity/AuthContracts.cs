namespace Portfolio.Api.Contracts.Identity;

/// <summary>
/// Request model for user registration.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password (min 8 characters, must include uppercase, lowercase, and number).</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);

/// <summary>
/// Response model for successful user registration.
/// </summary>
/// <param name="UserId">The unique identifier of the newly created user.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="FullName">The user's full name.</param>
public sealed record RegisterUserResponse(
    Guid UserId,
    string Email,
    string FullName);

/// <summary>
/// Request model for user login.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The user's password.</param>
public sealed record LoginRequest(
    string Email,
    string Password);

/// <summary>
/// Response model for successful login.
/// </summary>
/// <param name="Token">The JWT bearer token for authentication.</param>
/// <param name="UserId">The unique identifier of the user.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="FullName">The user's full name.</param>
/// <param name="Roles">The roles assigned to the user.</param>
public sealed record LoginResponse(
    string Token,
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles);
