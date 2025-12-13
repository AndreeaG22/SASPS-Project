using Microsoft.EntityFrameworkCore.Storage;
using Tagging.Application.Interfaces;
using Tagging.Infrastructure.Data;

namespace Tagging.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for Tagging module.
/// </summary>
public class TaggingUnitOfWork : ITaggingUnitOfWork
{
    private readonly TaggingDbContext _context;
    private IDbContextTransaction? _transaction;
    private ITagRepository? _tagRepository;
    private IDocumentTagRepository? _documentTagRepository;

    public TaggingUnitOfWork(TaggingDbContext context)
    {
        _context = context;
    }

    public ITagRepository Tags
    {
        get
        {
            _tagRepository ??= new TagRepository(_context);
            return _tagRepository;
        }
    }

    public IDocumentTagRepository DocumentTags
    {
        get
        {
            _documentTagRepository ??= new DocumentTagRepository(_context);
            return _documentTagRepository;
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
