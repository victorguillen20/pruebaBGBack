using BG.Invoice.Domain.Common;
using BG.Invoice.Domain.Exceptions;

namespace BG.Invoice.Domain.Entities;

/// <summary>
/// Singleton configuration. There is only ONE row in the DB (Id = 1).
/// Stores company info, tax percent, currency, and the atomic counter for invoice numbers.
/// </summary>
public class CompanyConfig : Entity
{
    public int Id { get; private set; } = 1;  // singleton
    public string CompanyName { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? TaxId { get; private set; }
    public decimal TaxPercent { get; private set; } = 13m;
    public string CurrencySymbol { get; private set; } = "$";
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Region { get; private set; }
    public string? PostalCode { get; private set; }
    public string? LogoUrl { get; private set; }

    /// <summary>
    /// Atomic counter for invoice numbers. Updated via a serialized transaction in Infrastructure.
    /// </summary>
    public int LastInvoiceNumber { get; private set; }

    private CompanyConfig() { }  // EF

    public static CompanyConfig Create(string companyName, decimal taxPercent = 13m, string currencySymbol = "$", string? phone = null, string? email = null, string? taxId = null, string? address = null, string? city = null, string? region = null, string? postalCode = null, string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(companyName)) throw new BusinessRuleException("CompanyName is required.");
        if (taxPercent < 0) throw new BusinessRuleException("TaxPercent cannot be negative.");
        if (string.IsNullOrWhiteSpace(currencySymbol)) throw new BusinessRuleException("CurrencySymbol is required.");
        return new CompanyConfig
        {
            Id = 1,
            CompanyName = companyName.Trim(),
            Phone = phone?.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            TaxId = taxId?.Trim(),
            TaxPercent = taxPercent,
            CurrencySymbol = currencySymbol.Trim(),
            Address = address?.Trim(),
            City = city?.Trim(),
            Region = region?.Trim(),
            PostalCode = postalCode?.Trim(),
            LogoUrl = logoUrl?.Trim(),
            LastInvoiceNumber = 0
        };
    }

    public void Update(string companyName, decimal taxPercent, string currencySymbol, string? phone, string? email, string? taxId, string? address, string? city, string? region, string? postalCode, string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(companyName)) throw new BusinessRuleException("CompanyName is required.");
        if (taxPercent < 0) throw new BusinessRuleException("TaxPercent cannot be negative.");
        if (string.IsNullOrWhiteSpace(currencySymbol)) throw new BusinessRuleException("CurrencySymbol is required.");
        CompanyName = companyName.Trim();
        TaxPercent = taxPercent;
        CurrencySymbol = currencySymbol.Trim();
        Phone = phone?.Trim();
        Email = email?.Trim().ToLowerInvariant();
        TaxId = taxId?.Trim();
        Address = address?.Trim();
        City = city?.Trim();
        Region = region?.Trim();
        PostalCode = postalCode?.Trim();
        LogoUrl = logoUrl?.Trim();
    }

    /// <summary>
    /// Atomically increment the invoice number counter.
    /// In Infrastructure, this is called inside a serialized transaction (BEGIN IMMEDIATE on SQLite).
    /// </summary>
    public int NextInvoiceNumber()
    {
        LastInvoiceNumber++;
        return LastInvoiceNumber;
    }
}
