using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Entities;

namespace BG.Invoice.Application.Mappings;

public static class MenuMappings
{
    public static MenuResponse ToResponse(this Menu menu)
    {
        return new MenuResponse(menu.Id, menu.Key, menu.Label, menu.Icon, menu.Route, menu.Order);
    }
}
