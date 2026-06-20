using BG.Invoice.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }
    public Task<Product?> GetByCodeAsync(string code, CancellationToken ct = default)
        => DbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Code == code, ct);
    public Task<Product?> GetByIdWithCategoryAsync(int id, CancellationToken ct = default)
        => DbSet.AsNoTracking().Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);
    public async Task<(IReadOnlyList<Product> Items, int Total)> SearchPagedAsync(string? search, int? categoryId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(p => p.IsActive);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Code.Contains(search));
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(p => p.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
    public async Task<(IReadOnlyList<Product> Items, int Total)> SearchPagedWithCategoryAsync(string? search, int? categoryId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().Include(p => p.Category).AsQueryable();
        if (activeOnly) query = query.Where(p => p.IsActive);
        if (categoryId.HasValue) query = query.Where(p => p.CategoryId == categoryId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Code.Contains(search));
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(p => p.Name).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
    public Task<int> CountActiveByCategoryAsync(int categoryId, CancellationToken ct = default)
        => DbSet.AsNoTracking().CountAsync(p => p.CategoryId == categoryId && p.IsActive, ct);
    public async Task<bool> TryDecrementStockAsync(int productId, int quantity, CancellationToken ct = default)
    {
        var product = await DbSet.FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null || product.Stock < quantity) return false;
        product.DecrementStock(quantity);
        await Context.SaveChangesAsync(ct);
        return true;
    }
    public async Task IncrementStockAsync(int productId, int quantity, CancellationToken ct = default)
    {
        var product = await DbSet.FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null) return;
        product.IncrementStock(quantity);
        await Context.SaveChangesAsync(ct);
    }
}
