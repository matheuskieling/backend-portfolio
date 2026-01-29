using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Users;

public sealed record AssignRoleToUserCommand(Guid UserId, Guid RoleId);

public sealed class AssignRoleToUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AssignRoleToUserHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task HandleAsync(
        AssignRoleToUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Privacy protection: users can only manage their own roles
        // Even admins cannot modify other users' roles in this portfolio application
        if (currentUserId != command.UserId)
        {
            throw new UnauthorizedAccessException("You can only manage your own roles.");
        }

        var user = await _userRepository.GetByIdWithRolesAsync(command.UserId, cancellationToken)
            ?? throw new UserNotFoundException(command.UserId);

        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken)
            ?? throw new RoleNotFoundException(command.RoleId);

        user.AssignRole(role);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
