namespace BG.Invoice.Application.Dtos;

public record CategoryResponse(
    int Id,
    string Name,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateCategoryRequest(string Name);

public record UpdateCategoryRequest(string Name);
