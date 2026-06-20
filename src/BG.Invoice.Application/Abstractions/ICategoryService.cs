using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface ICategoryService
{
    Task<Result<CategoryResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<CategoryResponse>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<Result<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task<Result> DeactivateAsync(int id, CancellationToken ct = default);
}
