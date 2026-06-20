using BG.Invoice.Domain.Common;
using BG.Invoice.Domain.Enums;
using BG.Invoice.Domain.Exceptions;

namespace BG.Invoice.Domain.Entities;

public class Payment : Entity
{
    public int Id { get; private set; }
    public int InvoiceId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public decimal Amount { get; private set; }
    public string? Reference { get; private set; }
    public DateTime PaymentDate { get; private set; }

    // Navigation
    public Invoice Invoice { get; private set; } = default!;

    private Payment() { }  // EF

    public Payment(int invoiceId, PaymentMethod method, decimal amount, string? reference, DateTime paymentDateUtc)
    {
        if (amount <= 0) throw new BusinessRuleException("Payment amount must be positive.");
        InvoiceId = invoiceId;
        Method = method;
        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        Reference = reference?.Trim();
        PaymentDate = DateTime.SpecifyKind(paymentDateUtc, DateTimeKind.Utc);
    }
}
