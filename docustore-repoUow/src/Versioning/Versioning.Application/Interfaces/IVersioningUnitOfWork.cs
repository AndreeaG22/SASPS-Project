namespace Versioning.Application.Interfaces;

/// <summary>
/// Unit of Work interface for Versioning module.
/// </summary>
public interface IVersioningUnitOfWork : IDisposable
{
    IVersionRepository Versions { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
