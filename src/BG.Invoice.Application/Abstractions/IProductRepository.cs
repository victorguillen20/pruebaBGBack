using BG.Invoice.Domain.Entities;

namespace BG.Invoice.Application.Abstractions;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Product?> GetByIdWithCategoryAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int Total)> SearchPagedAsync(string? search, int? categoryId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default);
    Task<(IReadOnlyList<Product> Items, int Total)> SearchPagedWithCategoryAsync(string? search, int? categoryId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default);
    Task<int> CountActiveByCategoryAsync(int categoryId, CancellationToken ct = default);
    Task<bool> TryDecrementStockAsync(int productId, int quantity, CancellationToken ct = default);
    Task IncrementStockAsync(int productId, int quantity, CancellationToken ct = default);
}
