using BG.Invoice.Domain.Common;

namespace BG.Invoice.Domain.Entities;

public class Role : Entity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<User> Users { get; private set; } = new List<User>();
    public ICollection<RoleMenu> RoleMenus { get; private set; } = new List<RoleMenu>();

    private Role() { }

    public static Role Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.BusinessRuleException("Role name is required.");
        return new Role
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void UpdateDescription(string? description) => Description = description?.Trim();
}