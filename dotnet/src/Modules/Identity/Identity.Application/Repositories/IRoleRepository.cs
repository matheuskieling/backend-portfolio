using Identity.Domain.Entities;

namespace Identity.Application.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameWithPermissionsAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    void Add(Role role);
    void Update(Role role);
    void Remove(Role role);
}
