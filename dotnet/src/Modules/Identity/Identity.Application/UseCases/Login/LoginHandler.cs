using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Login;

public sealed class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResult> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailWithRolesAsync(command.Email, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException(command.Email);
        }

        user.EnsureCanLogin();

        var isValidPassword = _passwordHasher.Verify(command.Password, user.PasswordHash.Value);
        if (!isValidPassword)
        {
            user.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new InvalidPasswordException("Invalid password.");
        }

        user.RecordSuccessfulLogin();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResult(
            token,
            user.Id,
            user.Email.Value,
            user.FullName,
            user.GetRoleNames().ToList());
    }
}
