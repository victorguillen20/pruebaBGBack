using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Entities;

namespace BG.Invoice.Application.Mappings;

public static class InvoiceMappings
{
    public static InvoiceResponse ToResponse(this Domain.Entities.Invoice invoice, DateTime createdAt)
    {
        return new InvoiceResponse(
            invoice.Id, invoice.Number, invoice.Date,
            invoice.CustomerId, invoice.Customer?.Name ?? "",
            invoice.SellerId, invoice.Seller?.FullName ?? "",
            invoice.Type, invoice.DueDate, invoice.Status,
            invoice.Notes, invoice.Subtotal, invoice.TaxAmount, invoice.Total,
            invoice.Details.Select(MapDetail).ToList(),
            invoice.Payments.Select(MapPayment).ToList(),
            createdAt);
    }

    public static InvoiceSummaryResponse ToSummaryResponse(this Domain.Entities.Invoice invoice)
    {
        return new InvoiceSummaryResponse(
            invoice.Id, invoice.Number, invoice.Date,
            invoice.Customer?.Name ?? "", invoice.Type, invoice.Status, invoice.Total,
            invoice.Seller?.FullName ?? "");
    }

    private static InvoiceDetailResponse MapDetail(InvoiceDetail detail)
    {
        return new InvoiceDetailResponse(
            detail.Id, detail.ProductId,
            detail.ProductNameSnapshot, detail.ProductCodeSnapshot,
            detail.Quantity, detail.UnitPrice, detail.LineTotal);
    }

    private static PaymentResponse MapPayment(Payment payment)
    {
        return new PaymentResponse(
            payment.Id, payment.Method, payment.Amount,
            payment.Reference, payment.PaymentDate);
    }
}
