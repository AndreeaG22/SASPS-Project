namespace Tagging.Application.Interfaces;

/// <summary>
/// Unit of Work interface for Tagging module.
/// </summary>
public interface ITaggingUnitOfWork : IDisposable
{
    ITagRepository Tags { get; }
    IDocumentTagRepository DocumentTags { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
