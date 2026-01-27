using Identity.Application.UseCases.Login;
using Identity.Application.UseCases.RegisterUser;
using Identity.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Api.Common;
using Portfolio.Api.Configuration;

namespace Portfolio.Api.Controllers.Identity;

[ApiController]
[Route("api/identity")]
[Tags("Identity - Auth")]
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

    [HttpPost("register")]
    [EnableRateLimiting(RateLimitingConfiguration.AuthPolicy)]
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

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitingConfiguration.AuthPolicy)]
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

public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);

public sealed record RegisterUserResponse(
    Guid UserId,
    string Email,
    string FullName);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record LoginResponse(
    string Token,
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles);
