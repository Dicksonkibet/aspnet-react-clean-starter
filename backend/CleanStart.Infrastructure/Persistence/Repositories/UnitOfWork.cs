using System.Collections.Concurrent;
using CleanStart.Application.Common.Interfaces;
using CleanStart.Domain.Common;

namespace CleanStart.Infrastructure.Persistence.Repositories;

/// <summary>Lazily creates and caches one EfRepository&lt;T&gt; per entity type
/// requested, and wraps every write behind a single SaveChangesAsync for atomic
/// commits.</summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(AppDbContext context) => _context = context;

    public IRepository<T> Repository<T>() where T : BaseEntity =>
        (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new EfRepository<T>(_context));

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
