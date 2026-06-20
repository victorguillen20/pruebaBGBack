namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }
    public Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default)
        => DbSet.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == userName, ct);
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => DbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);
    public async Task<(IReadOnlyList<User> Items, int Total)> SearchPagedAsync(string? search, int? roleId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(u => u.IsActive);
        if (roleId.HasValue) query = query.Where(u => u.RoleId == roleId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.UserName.Contains(search) || u.Email.Contains(search) || u.FirstName.Contains(search) || u.LastName.Contains(search));
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(u => u.UserName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
