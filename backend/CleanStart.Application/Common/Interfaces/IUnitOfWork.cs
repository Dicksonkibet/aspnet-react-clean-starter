using CleanStart.Domain.Common;

namespace CleanStart.Application.Common.Interfaces;

/// <summary>
/// Wraps every repository plus a single SaveChangesAsync call, so writes spanning
/// multiple entities commit atomically in one database transaction.
/// </summary>
public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
