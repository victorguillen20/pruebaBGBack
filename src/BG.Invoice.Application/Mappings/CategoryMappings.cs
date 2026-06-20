using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Entities;

namespace BG.Invoice.Application.Mappings;

public static class CategoryMappings
{
    public static CategoryResponse ToResponse(this Category category, DateTime createdAt)
    {
        return new CategoryResponse(category.Id, category.Name, category.IsActive, createdAt);
    }
}
