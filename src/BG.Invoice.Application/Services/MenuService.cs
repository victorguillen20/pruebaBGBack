using BG.Invoice.Application.Abstractions;
using BG.Invoice.Application.Dtos;
using BG.Invoice.Application.Mappings;
using BG.Invoice.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BG.Invoice.Application.Services;

public class MenuService : IMenuService
{
    private readonly IRepository<Menu> _repository;
    private readonly ILogger<MenuService> _logger;

    public MenuService(IRepository<Menu> repository, ILogger<MenuService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<MenuResponse>> GetMenusForRoleAsync(int roleId, CancellationToken ct = default)
    {
        var menus = await _repository.ListAsync(m =>
            m.IsActive && m.RoleMenus.Any(rm => rm.RoleId == roleId), ct);
        return menus.OrderBy(m => m.Order).Select(m => m.ToResponse()).ToList();
    }
}
