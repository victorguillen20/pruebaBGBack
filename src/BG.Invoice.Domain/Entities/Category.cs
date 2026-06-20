using BG.Invoice.Domain.Common;
using BG.Invoice.Domain.Exceptions;

namespace BG.Invoice.Domain.Entities;

public class Category : Entity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    // Navigation
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() { }  // EF

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

    /// <summary>
    /// Soft-delete via IsActive. Throws if there are active products — caller must handle the 409 response in the controller.
    /// </summary>
    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
