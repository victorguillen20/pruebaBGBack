namespace BG.Invoice.Application.Dtos;

public record DashboardSummaryResponse(
    int TotalInvoices,
    int TotalCustomers,
    int TotalProducts,
    int TotalActiveProducts,
    decimal TotalRevenue,
    decimal PendingAmount,
    decimal PaidAmount,
    int DraftInvoices,
    int PaidInvoices,
    int CancelledInvoices,
    IReadOnlyList<DashboardRecentInvoice> RecentInvoices,
    IReadOnlyList<DashboardTopProduct> TopProducts
);

public record DashboardRecentInvoice(
    int Id,
    int Number,
    DateTime Date,
    string CustomerName,
    string SellerName,
    decimal Total,
    string Status
);

public record DashboardTopProduct(
    int ProductId,
    string ProductName,
    string ProductCode,
    int TotalQuantitySold,
    decimal TotalRevenue
);
