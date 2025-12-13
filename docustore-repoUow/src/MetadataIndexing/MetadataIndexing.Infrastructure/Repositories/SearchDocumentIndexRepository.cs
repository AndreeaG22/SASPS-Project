using MetadataIndexing.Application.Interfaces;
using MetadataIndexing.Domain.Entities;
using MetadataIndexing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetadataIndexing.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for SearchDocumentIndex entity operations.
/// </summary>
public class SearchDocumentIndexRepository : ISearchDocumentIndexRepository
{
    private readonly MetadataIndexingDbContext _context;

    public SearchDocumentIndexRepository(MetadataIndexingDbContext context)
    {
        _context = context;
    }

    public async Task<SearchDocumentIndex?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SearchDocumentIndexes.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<SearchDocumentIndex?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.SearchDocumentIndexes
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
    }

    public async Task<List<SearchDocumentIndex>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SearchDocumentIndexes
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SearchDocumentIndex index, CancellationToken cancellationToken = default)
    {
        await _context.SearchDocumentIndexes.AddAsync(index, cancellationToken);
    }

    public void Update(SearchDocumentIndex index)
    {
        _context.SearchDocumentIndexes.Update(index);
    }
}
