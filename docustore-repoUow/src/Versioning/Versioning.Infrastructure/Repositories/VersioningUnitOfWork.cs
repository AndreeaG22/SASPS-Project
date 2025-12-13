using Microsoft.EntityFrameworkCore.Storage;
using Versioning.Application.Interfaces;
using Versioning.Infrastructure.Data;

namespace Versioning.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for Versioning module.
/// </summary>
public class VersioningUnitOfWork : IVersioningUnitOfWork
{
    private readonly VersioningDbContext _context;
    private IDbContextTransaction? _transaction;
    private IVersionRepository? _versionRepository;

    public VersioningUnitOfWork(VersioningDbContext context)
    {
        _context = context;
    }

    public IVersionRepository Versions
    {
        get
        {
            _versionRepository ??= new VersionRepository(_context);
            return _versionRepository;
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
