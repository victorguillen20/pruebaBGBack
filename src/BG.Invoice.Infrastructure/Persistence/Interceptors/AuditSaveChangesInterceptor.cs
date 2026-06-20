using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace BG.Invoice.Infrastructure.Persistence.Interceptors;
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TimeProvider _timeProvider;
    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor, TimeProvider timeProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _timeProvider = timeProvider;
    }
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    private void ApplyAudit(DbContext? context)
    {
        if (context == null) return;
        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var currentUserId = GetCurrentUserId();
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditable) continue;
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property("CreatedAt").CurrentValue = nowUtc;
                    entry.Property("CreatedBy").CurrentValue = currentUserId;
                    break;
                case EntityState.Modified:
                    entry.Property("ModifiedAt").CurrentValue = nowUtc;
                    entry.Property("ModifiedBy").CurrentValue = currentUserId;
                    break;
            }
        }
    }
    private int GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var sub = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user?.FindFirst("sub")?.Value
                  ?? user?.FindFirst("nameid")?.Value;
        return int.TryParse(sub, out var id) ? id : 0;
    }
}
