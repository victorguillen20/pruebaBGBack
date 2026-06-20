using BG.Invoice.Domain.Entities;

namespace BG.Invoice.Application.Abstractions;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUserNameWithRoleAsync(string userName, CancellationToken ct = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdWithRoleAsync(int id, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, int Total)> SearchPagedAsync(string? search, int? roleId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default);
}
