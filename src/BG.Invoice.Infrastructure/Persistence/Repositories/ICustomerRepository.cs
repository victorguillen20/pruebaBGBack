namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByIdentificationAsync(string identification, CancellationToken ct = default);
    Task<(IReadOnlyList<Customer> Items, int Total)> SearchPagedAsync(string? search, int page, int pageSize, bool activeOnly = true, CancellationToken ct = default);
}
