namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<bool> SaveChangesWithAuditAsync(CancellationToken ct = default);
}
