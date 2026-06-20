namespace BG.Invoice.Domain.Common;

/// <summary>
/// Abstract base class for all domain entities.
/// Implements <see cref="IAuditable"/> so audit shadow properties
/// (CreatedBy, CreatedAt, ModifiedBy, ModifiedAt) are configured
/// globally by the Infrastructure layer.
/// </summary>
public abstract class Entity : IAuditable
{
}
