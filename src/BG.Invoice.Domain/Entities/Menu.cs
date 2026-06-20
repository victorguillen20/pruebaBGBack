using BG.Invoice.Domain.Common;
using BG.Invoice.Domain.Exceptions;

namespace BG.Invoice.Domain.Entities;

public class Menu : Entity
{
    public int Id { get; private set; }
    public string Key { get; private set; } = default!;
    public string Label { get; private set; } = default!;
    public string Icon { get; private set; } = default!;
    public string Route { get; private set; } = default!;
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation
    public ICollection<RoleMenu> RoleMenus { get; private set; } = new List<RoleMenu>();

    private Menu() { }  // EF

    public static Menu Create(string key, string label, string icon, string route, int order = 0)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new BusinessRuleException("Menu Key is required.");
        if (string.IsNullOrWhiteSpace(label)) throw new BusinessRuleException("Menu Label is required.");
        if (string.IsNullOrWhiteSpace(icon)) throw new BusinessRuleException("Menu Icon is required.");
        if (string.IsNullOrWhiteSpace(route)) throw new BusinessRuleException("Menu Route is required.");
        if (order < 0) throw new BusinessRuleException("Menu Order cannot be negative.");

        return new Menu
        {
            Key = key.Trim(),
            Label = label.Trim(),
            Icon = icon.Trim(),
            Route = route.Trim(),
            Order = order,
            IsActive = true
        };
    }

    public void Update(string label, string icon, string route, int order)
    {
        if (string.IsNullOrWhiteSpace(label)) throw new BusinessRuleException("Menu Label is required.");
        if (string.IsNullOrWhiteSpace(icon)) throw new BusinessRuleException("Menu Icon is required.");
        if (string.IsNullOrWhiteSpace(route)) throw new BusinessRuleException("Menu Route is required.");
        if (order < 0) throw new BusinessRuleException("Menu Order cannot be negative.");
        Label = label.Trim();
        Icon = icon.Trim();
        Route = route.Trim();
        Order = order;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
