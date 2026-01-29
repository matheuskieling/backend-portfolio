using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Enums;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Users;

public sealed record GetCurrentUserQuery;

public sealed record GetCurrentUserResult(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    UserStatus Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed class GetCurrentUserHandler
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;

    public GetCurrentUserHandler(
        ICurrentUserService currentUserService,
        IUserRepository userRepository)
    {
        _currentUserService = currentUserService;
        _userRepository = userRepository;
    }

    public async Task<GetCurrentUserResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken)
            ?? throw new UserNotFoundException(userId);

        return new GetCurrentUserResult(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Status,
            user.CreatedAt,
            user.LastLoginAt,
            user.GetRoleNames().ToList(),
            user.GetPermissions().ToList());
    }
}
