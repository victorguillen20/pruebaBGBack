using BG.Invoice.Domain.Common;
using BG.Invoice.Domain.Enums;
using BG.Invoice.Domain.Exceptions;

namespace BG.Invoice.Domain.Entities;

public class Invoice : Entity
{
    public int Id { get; private set; }
    public int Number { get; private set; }
    public DateTime Date { get; private set; }
    public int CustomerId { get; private set; }
    public int SellerId { get; private set; }
    public InvoiceType Type { get; private set; } = InvoiceType.Contado;
    public DateTime? DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Pendiente;
    public string? Notes { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal Total { get; private set; }

    public Customer Customer { get; private set; } = default!;
    public User Seller { get; private set; } = default!;
    public ICollection<InvoiceDetail> Details { get; private set; } = new List<InvoiceDetail>();
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    private Invoice() { }

    public static Invoice Create(int number, DateTime dateUtc, int customerId, int sellerId, InvoiceType type, DateTime? dueDate = null, string? notes = null)
    {
        if (number <= 0) throw new BusinessRuleException("Invoice number must be positive.");
        if (customerId <= 0) throw new BusinessRuleException("CustomerId is required.");
        if (sellerId <= 0) throw new BusinessRuleException("SellerId is required.");
        if (type == InvoiceType.Credito && !dueDate.HasValue)
            throw new BusinessRuleException("Credit invoices require a DueDate.");

        return new Invoice
        {
            Number = number,
            Date = DateTime.SpecifyKind(dateUtc, DateTimeKind.Utc),
            CustomerId = customerId,
            SellerId = sellerId,
            Type = type,
            DueDate = dueDate.HasValue ? DateTime.SpecifyKind(dueDate.Value, DateTimeKind.Utc) : null,
            Status = InvoiceStatus.Pendiente,
            Notes = notes?.Trim(),
            Subtotal = 0m,
            TaxAmount = 0m,
            Total = 0m
        };
    }

    public void AddDetail(int productId, int quantity, decimal unitPrice, string productNameSnapshot, string productCodeSnapshot)
    {
        if (quantity <= 0) throw new BusinessRuleException("Detail quantity must be positive.");
        if (unitPrice < 0) throw new BusinessRuleException("Detail unit price cannot be negative.");
        if (Status == InvoiceStatus.Anulada)
            throw new BusinessRuleException("Cannot add details to a cancelled invoice.");

        var lineTotal = Math.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);
        var detail = InvoiceDetail.Create(productId, quantity, unitPrice, lineTotal, productNameSnapshot, productCodeSnapshot);
        detail.AttachToInvoice(Id);
        Details.Add(detail);
        RecalculateTotals();
    }

    public void RemoveDetail(int detailId)
    {
        var detail = Details.FirstOrDefault(d => d.Id == detailId);
        if (detail == null) return;
        Details.Remove(detail);
        RecalculateTotals();
    }

    public void AddPayment(PaymentMethod method, decimal amount, string? reference, DateTime paymentDateUtc)
    {
        if (amount <= 0) throw new BusinessRuleException("Payment amount must be positive.");
        if (Status == InvoiceStatus.Anulada)
            throw new BusinessRuleException("Cannot add payments to a cancelled invoice.");

        var payment = new Payment(Id, method, amount, reference?.Trim(), DateTime.SpecifyKind(paymentDateUtc, DateTimeKind.Utc));
        Payments.Add(payment);
        UpdateStatus();
    }

    public void SetTaxAmount(decimal taxAmount, decimal taxPercent)
    {
        if (taxAmount < 0) throw new BusinessRuleException("Tax amount cannot be negative.");
        if (taxPercent < 0) throw new BusinessRuleException("Tax percent cannot be negative.");
        TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero);
        Total = Math.Round(Subtotal + TaxAmount, 2, MidpointRounding.AwayFromZero);
    }

    public void SetNotes(string? notes) => Notes = notes?.Trim();

    public void Cancel()
    {
        if (Status == InvoiceStatus.Anulada) return;
        Status = InvoiceStatus.Anulada;
    }

    private void RecalculateTotals()
    {
        Subtotal = Math.Round(Details.Sum(d => d.LineTotal), 2, MidpointRounding.AwayFromZero);

        Total = Math.Round(Subtotal + TaxAmount, 2, MidpointRounding.AwayFromZero);
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        if (Status == InvoiceStatus.Anulada) return;
        var paid = Payments.Sum(p => p.Amount);
        if (paid >= Total && Total > 0) Status = InvoiceStatus.Pagada;
        else if (paid > 0 && paid < Total) Status = InvoiceStatus.Pendiente;
    }
}