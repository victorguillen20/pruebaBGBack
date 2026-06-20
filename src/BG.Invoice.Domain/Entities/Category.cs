using BG.Invoice.Domain.Common;
using BG.Invoice.Domain.Exceptions;

namespace BG.Invoice.Domain.Entities;

public class Category : Entity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() { }

    public static Category Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Category Name is required.");
        return new Category { Name = name.Trim(), IsActive = true };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Category Name is required.");
        Name = name.Trim();
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}