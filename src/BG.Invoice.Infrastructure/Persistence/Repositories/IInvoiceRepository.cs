using BG.Invoice.Domain.Enums;
namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public interface IInvoiceRepository : IRepository<global::BG.Invoice.Domain.Entities.Invoice>
{
    Task<global::BG.Invoice.Domain.Entities.Invoice?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<int> GetNextInvoiceNumberAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<global::BG.Invoice.Domain.Entities.Invoice> Items, int Total)> SearchPagedAsync(
        string? search, int? customerId, int? sellerId, InvoiceStatus? status,
        DateTime? fromDate, DateTime? toDate, decimal? minTotal, decimal? maxTotal,
        int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<global::BG.Invoice.Domain.Entities.Invoice>> GetRecentAsync(int count, CancellationToken ct = default);
    Task<Dictionary<int, decimal>> GetMonthlySalesByYearAsync(int year, CancellationToken ct = default);
    Task<IReadOnlyList<(int ProductId, string ProductName, int QuantitySold)>> GetTopProductsAsync(int count, CancellationToken ct = default);
}
