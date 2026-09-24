using CleanStart.Application.Common.Specifications;
using CleanStart.Domain.Common;

namespace CleanStart.Application.Common.Interfaces;

/// <summary>
/// One generic contract used for every entity in the system. New entities get full
/// CRUD support automatically — no per-entity repository interface needed.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T>? spec = null, CancellationToken ct = default);
    Task<int> CountAsync(ISpecification<T>? spec = null, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity); // soft delete, via BaseEntity.SoftDelete()
}
