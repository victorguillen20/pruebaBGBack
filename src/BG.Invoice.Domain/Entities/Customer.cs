using BG.Invoice.Domain.Common;
using BG.Invoice.Domain.Enums;
using BG.Invoice.Domain.Exceptions;

namespace BG.Invoice.Domain.Entities;

public class Customer : Entity
{
    public int Id { get; internal set; }
    public string Identification { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }
    public CustomerType Type { get; private set; } = CustomerType.Persona;
    public bool IsActive { get; private set; } = true;
    public decimal? CreditLimit { get; private set; }

    public ICollection<Invoice> Invoices { get; private set; } = new List<Invoice>();

    private Customer() { }

    public static Customer Create(string identification, string name, CustomerType type, string? phone = null, string? email = null, string? address = null, decimal? creditLimit = null)
    {
        if (string.IsNullOrWhiteSpace(identification)) throw new BusinessRuleException("Customer Identification is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Customer Name is required.");
        if (creditLimit.HasValue && creditLimit < 0) throw new BusinessRuleException("CreditLimit cannot be negative.");

        return new Customer
        {
            Identification = identification.Trim(),
            Name = name.Trim(),
            Type = type,
            Phone = phone?.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            Address = address?.Trim(),
            IsActive = true,
            CreditLimit = creditLimit
        };
    }

    public void Update(string name, CustomerType type, string? phone, string? email, string? address, decimal? creditLimit)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Customer Name is required.");
        if (creditLimit.HasValue && creditLimit < 0) throw new BusinessRuleException("CreditLimit cannot be negative.");
        Name = name.Trim();
        Type = type;
        Phone = phone?.Trim();
        Email = email?.Trim().ToLowerInvariant();
        Address = address?.Trim();
        CreditLimit = creditLimit;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}