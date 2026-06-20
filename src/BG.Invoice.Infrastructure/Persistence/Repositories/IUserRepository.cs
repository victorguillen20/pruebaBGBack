namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUserNameAsync(string userName, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, int Total)> SearchPagedAsync(string? search, int? roleId, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default);
}
