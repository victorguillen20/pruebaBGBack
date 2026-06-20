namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public interface IUnitOfWork : global::BG.Invoice.Application.Abstractions.IUnitOfWork
{
    new Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<bool> SaveChangesWithAuditAsync(CancellationToken ct = default);
}
