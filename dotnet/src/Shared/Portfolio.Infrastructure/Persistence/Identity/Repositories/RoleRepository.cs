using Identity.Application.Repositories;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Portfolio.Infrastructure.Persistence.Identity.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToUpperInvariant();
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == normalizedName, cancellationToken);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByNameWithPermissionsAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToUpperInvariant();
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == normalizedName, cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToUpperInvariant();
        return await _context.Roles
            .AnyAsync(r => r.Name == normalizedName, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles.ToListAsync(cancellationToken);
    }

    public void Add(Role role)
    {
        _context.Roles.Add(role);
    }

    public void Update(Role role)
    {
        _context.Roles.Update(role);
    }

    public void Remove(Role role)
    {
        _context.Roles.Remove(role);
    }
}
