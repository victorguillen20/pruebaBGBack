using BG.Invoice.Domain.Enums;

namespace BG.Invoice.Application.Dtos;

public record InvoiceResponse(
    int Id,
    int Number,
    DateTime Date,
    int CustomerId,
    string CustomerName,
    string? CustomerIdentification,
    string? CustomerPhone,
    string? CustomerEmail,
    int SellerId,
    string SellerName,
    InvoiceType Type,
    DateTime? DueDate,
    InvoiceStatus Status,
    string? Notes,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    IReadOnlyList<InvoiceDetailResponse> Details,
    IReadOnlyList<PaymentResponse> Payments,
    DateTime CreatedAt
);

public record InvoiceSummaryResponse(
    int Id,
    int Number,
    DateTime Date,
    string CustomerName,
    InvoiceType Type,
    InvoiceStatus Status,
    decimal Total,
    string? SellerName
);

public record InvoiceDetailResponse(
    int Id,
    int ProductId,
    string ProductName,
    string ProductCode,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

public record PaymentResponse(
    int Id,
    PaymentMethod Method,
    decimal Amount,
    string? Reference,
    DateTime PaymentDate
);

public record CreateInvoiceRequest(
    int CustomerId,
    InvoiceType Type,
    DateTime? DueDate,
    string? Notes,
    List<CreateInvoiceDetailRequest> Details
);

public record CreateInvoiceDetailRequest(
    int ProductId,
    int Quantity,
    decimal UnitPrice,
    string ProductName,
    string ProductCode
);

public record AddPaymentRequest(
    PaymentMethod Method,
    decimal Amount,
    string? Reference,
    DateTime? PaymentDate
);

public record InvoiceSearchCriteria(
    string? Search,
    int? CustomerId,
    int? SellerId,
    InvoiceStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate,
    decimal? MinTotal,
    decimal? MaxTotal,
    int Page,
    int PageSize
);
