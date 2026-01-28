using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Entities;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.RegisterUser;

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IIdentityUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var emailExists = await _userRepository.ExistsByEmailAsync(command.Email, cancellationToken);
        if (emailExists)
        {
            throw new UserAlreadyExistsException(command.Email);
        }

        var passwordHash = _passwordHasher.Hash(command.Password);

        var user = User.Create(
            command.Email,
            passwordHash,
            command.FirstName,
            command.LastName);

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserResult(
            user.Id,
            user.Email.Value,
            user.FullName);
    }
}
