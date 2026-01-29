using Common.Contracts;
using Common.Domain;
using Identity.Application.UseCases.Login;
using Identity.Application.UseCases.RegisterUser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Api.Configuration;
using Portfolio.Api.Contracts.Identity;

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
