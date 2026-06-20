using BG.Invoice.Domain.Common;
using BG.Invoice.Domain.Exceptions;

namespace BG.Invoice.Domain.Entities;

public class Product : Entity
{
    public int Id { get; internal set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public decimal? Cost { get; private set; }
    public int Stock { get; private set; }
    public int CategoryId { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Category? Category { get; internal set; } = default!;
    public ICollection<InvoiceDetail> InvoiceDetails { get; private set; } = new List<InvoiceDetail>();

    private Product() { }

    public static Product Create(string code, string name, decimal price, int categoryId, int stock = 0, decimal? cost = null, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new BusinessRuleException("Product Code is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Product Name is required.");
        if (price < 0) throw new BusinessRuleException("Product Price cannot be negative.");
        if (cost.HasValue && cost < 0) throw new BusinessRuleException("Product Cost cannot be negative.");
        if (stock < 0) throw new BusinessRuleException("Product Stock cannot be negative.");
        if (categoryId <= 0) throw new BusinessRuleException("CategoryId is required.");

        return new Product
        {
            Code = code.Trim(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Price = price,
            Cost = cost,
            Stock = stock,
            CategoryId = categoryId,
            IsActive = true
        };
    }

    public void Update(string name, decimal price, int categoryId, decimal? cost, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Product Name is required.");
        if (price < 0) throw new BusinessRuleException("Product Price cannot be negative.");
        if (cost.HasValue && cost < 0) throw new BusinessRuleException("Product Cost cannot be negative.");
        if (categoryId <= 0) throw new BusinessRuleException("CategoryId is required.");
        Name = name.Trim();
        Price = price;
        Cost = cost;
        CategoryId = categoryId;
        Description = description?.Trim();
    }

    public int DecrementStock(int quantity)
    {
        if (quantity <= 0) throw new BusinessRuleException("Decrement quantity must be positive.");
        if (Stock - quantity < 0)
            throw new BusinessRuleException($"Insufficient stock for product '{Code}'. Available: {Stock}, requested: {quantity}.");
        Stock -= quantity;
        return Stock;
    }

    public int IncrementStock(int quantity)
    {
        if (quantity <= 0) throw new BusinessRuleException("Increment quantity must be positive.");
        Stock += quantity;
        return Stock;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}