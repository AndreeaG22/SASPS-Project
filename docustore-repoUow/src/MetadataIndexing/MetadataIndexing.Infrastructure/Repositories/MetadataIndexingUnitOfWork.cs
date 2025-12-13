using MetadataIndexing.Application.Interfaces;
using MetadataIndexing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace MetadataIndexing.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for MetadataIndexing module.
/// </summary>
public class MetadataIndexingUnitOfWork : IMetadataIndexingUnitOfWork
{
    private readonly MetadataIndexingDbContext _context;
    private IDbContextTransaction? _transaction;
    private ISearchDocumentIndexRepository? _searchDocumentIndexRepository;

    public MetadataIndexingUnitOfWork(MetadataIndexingDbContext context)
    {
        _context = context;
    }

    public ISearchDocumentIndexRepository SearchDocumentIndexes
    {
        get
        {
            _searchDocumentIndexRepository ??= new SearchDocumentIndexRepository(_context);
            return _searchDocumentIndexRepository;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
