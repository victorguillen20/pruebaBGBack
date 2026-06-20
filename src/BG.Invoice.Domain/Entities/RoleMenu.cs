using BG.Invoice.Domain.Common;

namespace BG.Invoice.Domain.Entities;

/// <summary>
/// M:N join table between Role and Menu.
/// PK composed (MenuId, RoleId).
/// </summary>
public class RoleMenu : Entity
{
    public int MenuId { get; private set; }
    public int RoleId { get; private set; }

    // Navigation
    public Menu Menu { get; private set; } = default!;
    public Role Role { get; private set; } = default!;

    private RoleMenu() { }  // EF

    public RoleMenu(int menuId, int roleId)
    {
        MenuId = menuId;
        RoleId = roleId;
    }
}
