using CleanStart.Domain.Common;

namespace CleanStart.Domain.Entities;

/// <summary>
/// Sample entity proving the vertical slice (Domain -> Application -> Infrastructure ->
/// API -> React) works end to end. Delete once you add your own entities.
/// </summary>
public class TodoItem : BaseEntity
{
    public string Title { get; set; } = default!;
    public bool IsDone { get; set; }
}
