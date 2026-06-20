namespace BG.Invoice.Application.Dtos;

public record ProductResponse(
    int Id,
    string Code,
    string Name,
    string? Description,
    decimal Price,
    decimal? Cost,
    int Stock,
    int CategoryId,
    string CategoryName,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateProductRequest(
    string Code,
    string Name,
    decimal Price,
    int CategoryId,
    int Stock,
    decimal? Cost,
    string? Description
);

public record UpdateProductRequest(
    string Name,
    decimal Price,
    int CategoryId,
    decimal? Cost,
    string? Description
);
