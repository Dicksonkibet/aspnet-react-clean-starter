using CleanStart.Application.Common.Interfaces;
using CleanStart.Application.Common.Specifications;
using CleanStart.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CleanStart.Infrastructure.Persistence.Repositories;

/// <summary>Single implementation used for every entity in the system — no per-entity
/// repository classes needed. New entities get full CRUD automatically.</summary>
public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _context;
    public EfRepository(AppDbContext context) => _context = context;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Set<T>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T>? spec = null, CancellationToken ct = default) =>
        await SpecificationEvaluator.GetQuery(_context.Set<T>().AsQueryable(), spec).ToListAsync(ct);

    public async Task<int> CountAsync(ISpecification<T>? spec = null, CancellationToken ct = default) =>
        await SpecificationEvaluator.GetQuery(_context.Set<T>().AsQueryable(), spec).CountAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await _context.Set<T>().AddAsync(entity, ct);

    public void Update(T entity)
    {
        entity.MarkUpdated();
        _context.Set<T>().Update(entity);
    }

    public void Remove(T entity) => entity.SoftDelete(); // soft delete, never a physical DELETE
}
