using Identity.Application.UseCases.Login;
using Identity.Application.UseCases.RegisterUser;
using Identity.Domain.Common;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Register(
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

            return Created($"/api/identity/users/{result.UserId}", new
            {
                result.UserId,
                result.Email,
                result.FullName
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await _loginHandler.HandleAsync(command, cancellationToken);

            return Ok(new
            {
                result.Token,
                result.UserId,
                result.Email,
                result.FullName,
                result.Roles
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public sealed record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);

public sealed record LoginRequest(
    string Email,
    string Password);
