using BG.Invoice.Application.Dtos;
using BG.Invoice.Domain.Entities;

namespace BG.Invoice.Application.Mappings;

public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product, string categoryName, DateTime createdAt)
    {
        return new ProductResponse(
            product.Id, product.Code, product.Name, product.Description,
            product.Price, product.Cost, product.Stock,
            product.CategoryId, categoryName, product.IsActive, createdAt);
    }
}
