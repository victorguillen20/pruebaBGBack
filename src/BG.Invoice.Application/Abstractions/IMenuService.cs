using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface IMenuService
{
    Task<IReadOnlyList<MenuResponse>> GetMenusForRoleAsync(int roleId, CancellationToken ct = default);
}
