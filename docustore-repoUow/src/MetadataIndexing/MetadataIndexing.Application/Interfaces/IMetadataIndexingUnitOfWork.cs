namespace MetadataIndexing.Application.Interfaces;

/// <summary>
/// Unit of Work interface for MetadataIndexing module.
/// </summary>
public interface IMetadataIndexingUnitOfWork : IDisposable
{
    ISearchDocumentIndexRepository SearchDocumentIndexes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
