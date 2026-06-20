namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public interface IMenuRepository : IRepository<Menu>
{
    Task<IReadOnlyList<Menu>> GetActiveMenusForRoleAsync(int roleId, CancellationToken ct = default);
}
