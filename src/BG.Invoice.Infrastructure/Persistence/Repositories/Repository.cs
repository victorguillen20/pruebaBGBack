using System.Linq.Expressions;
namespace BG.Invoice.Infrastructure.Persistence.Repositories;
public class Repository<T> : IRepository<T>, global::BG.Invoice.Application.Abstractions.IRepository<T> where T : class, IAuditable
{
    protected AppDbContext Context { get; }
    protected DbSet<T> DbSet { get; }
    public Repository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await DbSet.FindAsync(new object?[] { id }, ct);
    public virtual async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? filter = null, CancellationToken ct = default)
    {
        IQueryable<T> query = DbSet.AsNoTracking();
        if (filter != null) query = query.Where(filter);
        return await query.ToListAsync(ct);
    }
    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await DbSet.AddAsync(entity, ct);
    public virtual void Update(T entity) => DbSet.Update(entity);
    public virtual void Remove(T entity) => DbSet.Remove(entity);
    public virtual Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Context.SaveChangesAsync(ct);
}
