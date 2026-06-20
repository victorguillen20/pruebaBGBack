using BG.Invoice.Application.Dtos;

namespace BG.Invoice.Application.Abstractions;

public interface IProductService
{
    Task<Result<ProductResponse>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<ProductResponse>> SearchAsync(string? search, int? categoryId, int page, int pageSize, CancellationToken ct = default);
    Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<Result<ProductResponse>> UpdateAsync(int id, UpdateProductRequest request, CancellationToken ct = default);
    Task<Result> DeactivateAsync(int id, CancellationToken ct = default);
    Task<Result> ActivateAsync(int id, CancellationToken ct = default);
}
