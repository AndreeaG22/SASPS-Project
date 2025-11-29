using Microsoft.EntityFrameworkCore;
using MetadataIndexing.Domain.Common;

namespace MetadataIndexing.Domain.Entities;

public class SearchDocumentIndex : ActiveRecordBase
{
    public Guid DocumentId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeInBytes { get; private set; }
    public bool IsDeleted { get; private set; }

    // EF Core requires a parameterless constructor
    private SearchDocumentIndex() { }

    public static SearchDocumentIndex Create(
        Guid documentId,
        string title,
        string? description,
        string fileName,
        string contentType,
        long fileSizeInBytes,
        string createdBy,
        DateTime createdAt)
    {
        var index = new SearchDocumentIndex
        {
            DocumentId = documentId,
            Title = title,
            Description = description,
            FileName = fileName,
            ContentType = contentType,
            FileSizeInBytes = fileSizeInBytes,
            IsDeleted = false
        };

        index.SetCreatedBy(createdBy);
        // Override CreatedAt with document's creation time
        index.CreatedAt = createdAt;
        
        return index;
    }

    public void UpdateMetadata(
        string title,
        string? description,
        string updatedBy)
    {
        Title = title;
        Description = description;
        SetUpdatedBy(updatedBy);
    }

    public void MarkAsDeleted(string deletedBy)
    {
        IsDeleted = true;
        SetUpdatedBy(deletedBy);
    }

    public async Task Save(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        var dbSet = context.Set<SearchDocumentIndex>();

        // Check if entity exists in database
        var exists = await dbSet.AnyAsync(d => d.Id == this.Id, cancellationToken);

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

    public static async Task<SearchDocumentIndex?> Find(Guid id, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<SearchDocumentIndex>().FindAsync(new object[] { id }, cancellationToken);
    }

    public static async Task<SearchDocumentIndex?> FindByDocumentId(Guid documentId, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<SearchDocumentIndex>()
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, cancellationToken);
    }

    public static async Task<List<SearchDocumentIndex>> All(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<SearchDocumentIndex>()
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
