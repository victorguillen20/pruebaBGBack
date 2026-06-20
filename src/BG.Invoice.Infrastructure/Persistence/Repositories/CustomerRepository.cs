namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }
    public Task<Customer?> GetByIdentificationAsync(string identification, CancellationToken ct = default)
        => DbSet.AsNoTracking().FirstOrDefaultAsync(c => c.Identification == identification, ct);
    public async Task<(IReadOnlyList<Customer> Items, int Total)> SearchPagedAsync(string? search, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{search}%") || EF.Functions.Like(c.Identification, $"%{search}%"));
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(c => c.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
