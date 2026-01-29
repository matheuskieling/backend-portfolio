using Identity.Application.Common.Interfaces;
using Identity.Application.Repositories;
using Identity.Domain.Enums;
using Identity.Domain.Exceptions;

namespace Identity.Application.UseCases.Admin.Users;

public sealed record GetUsersQuery;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    UserStatus Status,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles);

public sealed record GetUsersResult(IReadOnlyList<UserDto> Users);

public sealed class GetUsersHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetUsersHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetUsersResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        // Users can only see themselves
        var user = await _userRepository.GetByIdWithRolesAsync(currentUserId, cancellationToken)
            ?? throw new UserNotFoundException(currentUserId);

        var dto = new UserDto(
            user.Id,
            user.Email.Value,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Status,
            user.CreatedAt,
            user.LastLoginAt,
            user.GetRoleNames().ToList());

        return new GetUsersResult(new[] { dto });
    }
}
