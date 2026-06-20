namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public class MenuRepository : Repository<Menu>, IMenuRepository
{
    public MenuRepository(AppDbContext context) : base(context) { }
    public async Task<IReadOnlyList<Menu>> GetActiveMenusForRoleAsync(int roleId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(m => m.IsActive && m.RoleMenus.Any(rm => rm.RoleId == roleId))
            .OrderBy(m => m.Order)
            .ToListAsync(ct);
}
