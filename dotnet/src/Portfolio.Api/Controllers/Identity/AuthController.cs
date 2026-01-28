using System.Net;
using Common.Contracts;
using Identity.Application.UseCases.Login;
using Identity.Application.UseCases.RegisterUser;
using Common.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Api.Configuration;

namespace Portfolio.Api.Controllers.Identity;

/// <summary>
/// Handles user authentication operations including registration and login.
/// </summary>
[ApiController]
[Route("api/identity")]
[Tags("Identity - Auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _registerUserHandler;
    private readonly LoginHandler _loginHandler;

    public AuthController(
        RegisterUserHandler registerUserHandler,
        LoginHandler loginHandler)
    {
        _registerUserHandler = registerUserHandler;
        _loginHandler = loginHandler;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="request">The registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created user information.</returns>
    /// <response code="201">User successfully registered.</response>
    /// <response code="400">Invalid request data or domain validation error.</response>
    /// <response code="409">A user with this email already exists.</response>
    /// <response code="429">Too many requests. Rate limit exceeded.</response>
    [HttpPost("register")]
    [EnableRateLimiting(RateLimitingConfiguration.AuthPolicy)]
    [ProducesResponseType(typeof(ApiResponse<RegisterUserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<RegisterUserResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<RegisterUserResponse>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ApiResponse<RegisterUserResponse>> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName);

            var result = await _registerUserHandler.HandleAsync(command, cancellationToken);

            return ApiResponse.Created(new RegisterUserResponse(
                result.UserId,
                result.Email,
                result.FullName));
        }
        catch (DomainException ex)
        {
            return ApiResponse.Failure<RegisterUserResponse>("DOMAIN_ERROR", ex.Message);
        }
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT token and user information.</returns>
    /// <response code="200">Successfully authenticated.</response>
    /// <response code="400">Invalid credentials or domain validation error.</response>
    /// <response code="429">Too many requests. Rate limit exceeded.</response>
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitingConfiguration.AuthPolicy)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ApiResponse<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await _loginHandler.HandleAsync(command, cancellationToken);

            return ApiResponse.Success(new LoginResponse(
                result.Token,
                result.UserId,
                result.Email,
                result.FullName,
                result.Roles));
        }
        catch (DomainException ex)
        {
            return ApiResponse.Failure<LoginResponse>("DOMAIN_ERROR", ex.Message);
        }
    }
}

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
