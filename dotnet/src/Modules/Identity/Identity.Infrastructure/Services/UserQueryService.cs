using Common.Contracts.Identity;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Services;

public class UserQueryService : IUserQueryService
{
    private readonly IdentityDbContext _dbContext;

    public UserQueryService(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserBasicInfoDto?> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserBasicInfoDto(
                u.Id,
                u.Email.Value,
                u.FirstName,
                u.LastName,
                u.FullName))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyDictionary<Guid, UserBasicInfoDto>> GetByIdsAsync(
        IEnumerable<Guid> userIds, CancellationToken ct = default)
    {
        var idList = userIds.Distinct().ToList();

        if (idList.Count == 0)
            return new Dictionary<Guid, UserBasicInfoDto>();

        var users = await _dbContext.Users
            .Where(u => idList.Contains(u.Id))
            .Select(u => new UserBasicInfoDto(
                u.Id,
                u.Email.Value,
                u.FirstName,
                u.LastName,
                u.FullName))
            .ToListAsync(ct);

        return users.ToDictionary(u => u.Id);
    }
}
