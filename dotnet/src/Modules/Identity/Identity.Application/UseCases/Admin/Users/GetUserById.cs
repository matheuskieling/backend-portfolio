using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Enums;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Users;

public sealed record GetUserByIdQuery(Guid Id);

public sealed record GetUserByIdResult(
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

public sealed class GetUserByIdHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUserByIdHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetUserByIdResult> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Check authorization: admin can view anyone, others can only view themselves
        var isSelf = currentUserId == query.Id;
        if (!_currentUserService.IsAdmin && !isSelf)
        {
            throw new UnauthorizedAccessException("You can only view your own user information.");
        }

        var user = await _userRepository.GetByIdWithRolesAsync(query.Id, cancellationToken)
            ?? throw new UserNotFoundException(query.Id);

        return new GetUserByIdResult(
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
