using BG.Invoice.Domain.Common;

namespace BG.Invoice.Domain.Entities;

public class RoleMenu : Entity
{
    public int MenuId { get; private set; }
    public int RoleId { get; private set; }

    public Menu Menu { get; private set; } = default!;
    public Role Role { get; private set; } = default!;

    private RoleMenu() { }

    public RoleMenu(int menuId, int roleId)
    {
        MenuId = menuId;
        RoleId = roleId;
    }
}