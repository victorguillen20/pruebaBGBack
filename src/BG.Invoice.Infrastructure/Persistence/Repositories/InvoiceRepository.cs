using System.Data;
using BG.Invoice.Domain.Enums;
namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public class InvoiceRepository : Repository<global::BG.Invoice.Domain.Entities.Invoice>, IInvoiceRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    public InvoiceRepository(AppDbContext context, IDbContextFactory<AppDbContext> contextFactory) : base(context)
    {
        _contextFactory = contextFactory;
    }
    public async Task<global::BG.Invoice.Domain.Entities.Invoice?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default)
        => await DbSet
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Seller)
            .Include(i => i.Details).ThenInclude(d => d.Product)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    public async Task<int> GetNextInvoiceNumberAsync(CancellationToken ct = default)
    {
        await using var separateContext = await _contextFactory.CreateDbContextAsync(ct);
        await using var tx = await separateContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var config = await separateContext.CompanyConfig.FirstAsync(ct);
        var newNumber = config.NextInvoiceNumber();
        await separateContext.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return newNumber;
    }
    public async Task<(IReadOnlyList<global::BG.Invoice.Domain.Entities.Invoice> Items, int Total)> SearchPagedAsync(
        string? search, int? customerId, int? sellerId, InvoiceStatus? status,
        DateTime? fromDate, DateTime? toDate, decimal? minTotal, decimal? maxTotal,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();
        if (customerId.HasValue) query = query.Where(i => i.CustomerId == customerId);
        if (sellerId.HasValue) query = query.Where(i => i.SellerId == sellerId);
        if (status.HasValue) query = query.Where(i => i.Status == status);
        if (fromDate.HasValue) query = query.Where(i => i.Date >= fromDate);
        if (toDate.HasValue) query = query.Where(i => i.Date <= toDate);
        if (minTotal.HasValue) query = query.Where(i => i.Total >= minTotal);
        if (maxTotal.HasValue) query = query.Where(i => i.Total <= maxTotal);
        if (!string.IsNullOrWhiteSpace(search))
        {
            if (int.TryParse(search, out var n))
                query = query.Where(i => i.Number == n);
            else
                query = query.Where(i => i.Customer!.Name.Contains(search));
        }
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(i => i.Date).ThenByDescending(i => i.Number)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }
    public async Task<IReadOnlyList<global::BG.Invoice.Domain.Entities.Invoice>> GetRecentAsync(int count, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Include(i => i.Customer)
            .OrderByDescending(i => i.Date).ThenByDescending(i => i.Number)
            .Take(count)
            .ToListAsync(ct);
    public async Task<Dictionary<int, decimal>> GetMonthlySalesByYearAsync(int year, CancellationToken ct = default)
    {
        var fromDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var toDate = fromDate.AddYears(1);
        var sales = await DbSet.AsNoTracking()
            .Where(i => i.Date >= fromDate && i.Date < toDate && i.Status != InvoiceStatus.Anulada)
            .GroupBy(i => i.Date.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(i => i.Total) })
            .ToListAsync(ct);
        return sales.ToDictionary(s => s.Month, s => s.Total);
    }
    public async Task<IReadOnlyList<(int ProductId, string ProductName, int QuantitySold)>> GetTopProductsAsync(int count, CancellationToken ct = default)
    {
        var fromDate = DateTime.UtcNow.AddYears(-1);
        var top = await DbSet.AsNoTracking()
            .Where(i => i.Date >= fromDate && i.Status != InvoiceStatus.Anulada)
            .SelectMany(i => i.Details)
            .GroupBy(d => d.ProductId)
            .Select(g => new { ProductId = g.Key, QuantitySold = g.Sum(d => d.Quantity) })
            .OrderByDescending(x => x.QuantitySold)
            .Take(count)
            .Join(Context.Set<Product>().IgnoreQueryFilters(), x => x.ProductId, p => p.Id, (x, p) => new { x.ProductId, p.Name, x.QuantitySold })
            .ToListAsync(ct);
        return top.Select(t => (t.ProductId, t.Name, t.QuantitySold)).ToList();
    }
}
