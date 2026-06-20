namespace BG.Invoice.Domain.Common;

/// <summary>
/// Marker interface for entities that have audit fields.
/// The properties themselves are EF Core shadow properties
/// (set by AuditSaveChangesInterceptor in Infrastructure).
/// </summary>
public interface IAuditable
{
}
