using BG.Invoice.Domain.Enums;

namespace BG.Invoice.Application.Abstractions;

public interface IInvoiceRepository : IRepository<BG.Invoice.Domain.Entities.Invoice>
{
    Task<BG.Invoice.Domain.Entities.Invoice?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
    Task<int> GetNextInvoiceNumberAsync(CancellationToken ct = default);
    Task<(IReadOnlyList<BG.Invoice.Domain.Entities.Invoice> Items, int Total)> SearchPagedAsync(
        string? search, int? customerId, int? sellerId, InvoiceStatus? status,
        DateTime? fromDate, DateTime? toDate, decimal? minTotal, decimal? maxTotal,
        int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<BG.Invoice.Domain.Entities.Invoice>> GetRecentAsync(int count, CancellationToken ct = default);
    Task<Dictionary<int, decimal>> GetMonthlySalesByYearAsync(int year, CancellationToken ct = default);
    Task<IReadOnlyList<BG.Invoice.Domain.Entities.TopProductRow>> GetTopProductsAsync(int count, CancellationToken ct = default);
}
