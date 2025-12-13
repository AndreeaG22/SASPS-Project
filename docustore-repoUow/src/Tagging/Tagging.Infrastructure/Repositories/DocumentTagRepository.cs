using Microsoft.EntityFrameworkCore;
using Tagging.Application.Interfaces;
using Tagging.Domain.Entities;
using Tagging.Infrastructure.Data;

namespace Tagging.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for DocumentTag entity operations.
/// </summary>
public class DocumentTagRepository : IDocumentTagRepository
{
    private readonly TaggingDbContext _context;

    public DocumentTagRepository(TaggingDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentTags
            .Include(dt => dt.Tag)
            .FirstOrDefaultAsync(dt => dt.Id == id, cancellationToken);
    }

    public async Task<DocumentTag?> GetByDocumentAndTagAsync(Guid documentId, Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentTags
            .Include(dt => dt.Tag)
            .FirstOrDefaultAsync(dt => dt.DocumentId == documentId && dt.TagId == tagId, cancellationToken);
    }

    public async Task<List<DocumentTag>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentTags
            .Include(dt => dt.Tag)
            .Where(dt => dt.DocumentId == documentId)
            .OrderBy(dt => dt.Tag.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DocumentTag>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentTags
            .Where(dt => dt.TagId == tagId)
            .OrderByDescending(dt => dt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DocumentTag>> GetByTagIdsAsync(List<Guid> tagIds, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentTags
            .Where(dt => tagIds.Contains(dt.TagId))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid documentId, Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _context.DocumentTags
            .AnyAsync(dt => dt.DocumentId == documentId && dt.TagId == tagId, cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetDocumentCountsByTagAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DocumentTags
            .GroupBy(dt => dt.TagId)
            .Select(g => new { TagId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TagId, x => x.Count, cancellationToken);
    }

    public async Task AddAsync(DocumentTag documentTag, CancellationToken cancellationToken = default)
    {
        await _context.DocumentTags.AddAsync(documentTag, cancellationToken);
    }

    public void Remove(DocumentTag documentTag)
    {
        _context.DocumentTags.Remove(documentTag);
    }
}
