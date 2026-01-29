using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Users;

public sealed record RemoveRoleFromUserCommand(Guid UserId, Guid RoleId);

public sealed class RemoveRoleFromUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RemoveRoleFromUserHandler(
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
        RemoveRoleFromUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Check authorization: admin can manage anyone, managers can only manage themselves
        var isSelfManagement = currentUserId == command.UserId;
        if (!_currentUserService.IsAdmin && !isSelfManagement)
        {
            throw new UnauthorizedAccessException("You can only manage your own roles.");
        }

        var user = await _userRepository.GetByIdWithRolesAsync(command.UserId, cancellationToken)
            ?? throw new UserNotFoundException(command.UserId);

        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken)
            ?? throw new RoleNotFoundException(command.RoleId);

        user.RemoveRole(role);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
