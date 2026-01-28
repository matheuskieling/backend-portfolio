namespace Common.Contracts.Identity;

public interface IUserQueryService
{
    Task<UserBasicInfoDto?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<Guid, UserBasicInfoDto>> GetByIdsAsync(
        IEnumerable<Guid> userIds, CancellationToken ct = default);
}
