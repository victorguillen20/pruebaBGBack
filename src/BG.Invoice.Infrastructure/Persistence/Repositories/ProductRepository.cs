namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }
    public Task<Product?> GetByCodeAsync(string code, CancellationToken ct = default)
        => DbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Code == code, ct);
    public async Task<(IReadOnlyList<Product> Items, int Total)> SearchPagedAsync(string? search, int? categoryId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(p => p.IsActive);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{search}%") || EF.Functions.Like(p.Code, $"%{search}%"));
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(p => p.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
    public Task<int> CountActiveByCategoryAsync(int categoryId, CancellationToken ct = default)
        => DbSet.AsNoTracking().CountAsync(p => p.CategoryId == categoryId && p.IsActive, ct);
    public async Task<bool> TryDecrementStockAsync(int productId, int quantity, CancellationToken ct = default)
    {
        var affected = await DbSet
            .Where(p => p.Id == productId && p.Stock >= quantity)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - quantity), ct);
        return affected > 0;
    }
    public async Task IncrementStockAsync(int productId, int quantity, CancellationToken ct = default)
    {
        await DbSet
            .Where(p => p.Id == productId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock + quantity), ct);
    }
}
