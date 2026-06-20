using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface IInvoiceService
{
    Task<Result<InvoiceResponse>> GetByIdAsync(int id, int requestingUserId, bool isAdmin, CancellationToken ct = default);
    Task<PagedResult<InvoiceSummaryResponse>> SearchAsync(InvoiceSearchCriteria criteria, int requestingUserId, bool isAdmin, CancellationToken ct = default);
    Task<Result<InvoiceResponse>> CreateAsync(CreateInvoiceRequest request, int userId, CancellationToken ct = default);
    Task<Result> CancelAsync(int id, int userId, CancellationToken ct = default);
    Task<Result> AddPaymentAsync(int invoiceId, AddPaymentRequest request, CancellationToken ct = default);
}
