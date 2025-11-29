using Microsoft.EntityFrameworkCore;
using Tagging.Domain.Common;

namespace Tagging.Domain.Entities;

public class DocumentTag : ActiveRecordBase
{
    public Guid DocumentId { get; private set; }
    public Guid TagId { get; private set; }

    // Navigation properties
    public Tag Tag { get; private set; } = null!;

    // EF Core requires a parameterless constructor
    private DocumentTag() { }

    public static DocumentTag Create(Guid documentId, Guid tagId, string createdBy)
    {
        var documentTag = new DocumentTag
        {
            DocumentId = documentId,
            TagId = tagId
        };

        documentTag.SetCreatedBy(createdBy);
        return documentTag;
    }

    public async Task Save(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        var dbSet = context.Set<DocumentTag>();

        // Check if entity exists in database
        var exists = await dbSet.AnyAsync(dt => dt.Id == this.Id, cancellationToken);

        if (exists)
        {
            dbSet.Update(this);
        }
        else
        {
            await dbSet.AddAsync(this, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task Delete(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        context.Set<DocumentTag>().Remove(this);
        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task<DocumentTag?> Find(Guid id, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentTag>()
            .Include(dt => dt.Tag)
            .FirstOrDefaultAsync(dt => dt.Id == id, cancellationToken);
    }

    public static async Task<DocumentTag?> FindByDocumentAndTag(Guid documentId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentTag>()
            .Include(dt => dt.Tag)
            .FirstOrDefaultAsync(dt => dt.DocumentId == documentId && dt.TagId == tagId, cancellationToken);
    }

    public static async Task<List<DocumentTag>> GetByDocument(Guid documentId, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentTag>()
            .Include(dt => dt.Tag)
            .Where(dt => dt.DocumentId == documentId)
            .OrderBy(dt => dt.Tag.Name)
            .ToListAsync(cancellationToken);
    }

    public static async Task<List<DocumentTag>> GetByTag(Guid tagId, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentTag>()
            .Where(dt => dt.TagId == tagId)
            .OrderByDescending(dt => dt.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public static async Task<List<DocumentTag>> GetByTags(List<Guid> tagIds, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentTag>()
            .Where(dt => tagIds.Contains(dt.TagId))
            .ToListAsync(cancellationToken);
    }

    public static async Task<bool> Exists(Guid documentId, Guid tagId, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentTag>()
            .AnyAsync(dt => dt.DocumentId == documentId && dt.TagId == tagId, cancellationToken);
    }

    public static async Task<Dictionary<Guid, int>> GetDocumentCountsByTag(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentTag>()
            .GroupBy(dt => dt.TagId)
            .Select(g => new { TagId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TagId, x => x.Count, cancellationToken);
    }
}
