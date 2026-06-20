using BG.Invoice.Domain.Common;
using BG.Invoice.Domain.Exceptions;

namespace BG.Invoice.Domain.Entities;

public class InvoiceDetail : Entity
{
    public int Id { get; private set; }
    public int InvoiceId { get; private set; }
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal { get; private set; }

    // Snapshot of product info at the time of invoicing (for display in PDF even if product is later deleted/renamed)
    public string ProductNameSnapshot { get; private set; } = default!;
    public string ProductCodeSnapshot { get; private set; } = default!;

    // Navigation
    public Invoice Invoice { get; private set; } = default!;
    public Product Product { get; private set; } = default!;

    private InvoiceDetail() { }  // EF

    public static InvoiceDetail Create(int productId, int quantity, decimal unitPrice, decimal lineTotal, string productNameSnapshot, string productCodeSnapshot)
    {
        if (quantity <= 0) throw new BusinessRuleException("Detail quantity must be positive.");
        if (unitPrice < 0) throw new BusinessRuleException("Detail unit price cannot be negative.");
        if (lineTotal < 0) throw new BusinessRuleException("Detail line total cannot be negative.");
        if (string.IsNullOrWhiteSpace(productNameSnapshot)) throw new BusinessRuleException("ProductNameSnapshot is required.");
        if (string.IsNullOrWhiteSpace(productCodeSnapshot)) throw new BusinessRuleException("ProductCodeSnapshot is required.");

        return new InvoiceDetail
        {
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = lineTotal,
            ProductNameSnapshot = productNameSnapshot.Trim(),
            ProductCodeSnapshot = productCodeSnapshot.Trim()
        };
    }

    public void AttachToInvoice(int invoiceId)
    {
        InvoiceId = invoiceId;
    }
}
