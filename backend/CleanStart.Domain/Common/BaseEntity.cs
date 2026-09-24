namespace CleanStart.Domain.Common;

/// <summary>
/// Base class inherited by every entity in the system. Centralizes the Id and audit
/// fields so individual entities never redeclare them (DRY), and gives the generic
/// repository/unit-of-work in Application+Infrastructure one shape to work against.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    public void MarkUpdated() => UpdatedAt = DateTime.UtcNow;

    /// <summary>Soft delete — rows are never physically removed.</summary>
    public void SoftDelete() => IsDeleted = true;
}
