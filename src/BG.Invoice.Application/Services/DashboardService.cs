using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Entities;
using BG.Invoice.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        IInvoiceRepository invoiceRepository,
        IRepository<Customer> customerRepository,
        IRepository<Product> productRepository,
        ILogger<DashboardService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Result<DashboardSummaryResponse>> GetSummaryAsync(int requestingUserId, bool isAdmin, CancellationToken ct = default)
    {
        var allInvoices = await _invoiceRepository.ListAsync(ct: ct);
        var scopeInvoices = isAdmin ? allInvoices : allInvoices.Where(i => i.SellerId == requestingUserId).ToList();

        var totalInvoices = scopeInvoices.Count;
        var totalRevenue = scopeInvoices
            .Where(i => i.Status != InvoiceStatus.Anulada)
            .Sum(i => i.Total);
        var draftInvoices = scopeInvoices.Count(i => i.Status == InvoiceStatus.Pendiente);
        var paidInvoices = scopeInvoices.Count(i => i.Status == InvoiceStatus.Pagada);
        var cancelledInvoices = scopeInvoices.Count(i => i.Status == InvoiceStatus.Anulada);

        var paidAmount = scopeInvoices
            .Where(i => i.Status != InvoiceStatus.Anulada)
            .Sum(i => i.Payments.Sum(p => p.Amount));
        var pendingAmount = totalRevenue - paidAmount;

        var recentInvoicesRaw = await _invoiceRepository.GetRecentAsync(5, ct);
        var recentInvoices = recentInvoicesRaw
            .Where(i => isAdmin || i.SellerId == requestingUserId)
            .Take(5)
            .Select(i => new DashboardRecentInvoice(
                i.Id, i.Number, i.Date,
                i.Customer?.Name ?? "",
                i.Seller?.FullName ?? "",
                i.Total,
                i.Status.ToString()))
            .ToList();

        var topProducts = (await _invoiceRepository.GetTopProductsAsync(5, ct))
            .Select(t => new DashboardTopProduct(
                t.ProductId, t.ProductName, t.ProductCode,
                t.TotalQuantitySold, t.TotalRevenue))
            .ToList();

        var totalCustomers = await _customerRepository.ListAsync(c => c.IsActive, ct);
        var allProducts = await _productRepository.ListAsync(ct: ct);
        var activeProducts = allProducts.Count(p => p.IsActive);

        var summary = new DashboardSummaryResponse(
            TotalInvoices: totalInvoices,
            TotalCustomers: totalCustomers.Count,
            TotalProducts: allProducts.Count,
            TotalActiveProducts: activeProducts,
            TotalRevenue: totalRevenue,
            PendingAmount: pendingAmount,
            PaidAmount: paidAmount,
            DraftInvoices: draftInvoices,
            PaidInvoices: paidInvoices,
            CancelledInvoices: cancelledInvoices,
            RecentInvoices: recentInvoices,
            TopProducts: topProducts
        );

        return Result.Success(summary);
    }
}
