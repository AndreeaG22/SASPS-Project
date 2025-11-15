using Document.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Document.Domain.Entities;

public class DocumentEntity : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePathOnDisk { get; private set; } = string.Empty;
    public long FileSizeInBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;

    // EF Core requires a parameterless constructor
    private DocumentEntity() { }

    // Factory method for creating new documents
    public static DocumentEntity Create(
        string title,
        string? description,
        string fileName,
        string filePathOnDisk,
        long fileSizeInBytes,
        string contentType,
        string createdBy)
    {
        var document = new DocumentEntity
        {
            Title = title,
            Description = description,
            FileName = fileName,
            FilePathOnDisk = filePathOnDisk,
            FileSizeInBytes = fileSizeInBytes,
            ContentType = contentType
        };
        
        document.SetCreatedBy(createdBy);
        return document;
    }

    // Active Record: Save
    public async Task<DocumentEntity> SaveAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        await context.Set<DocumentEntity>().AddAsync(this, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return this;
    }

    // Active Record: Update
    public async Task UpdateAsync(DbContext context, string title, string? description, string updatedBy, CancellationToken cancellationToken = default)
    {
        Title = title;
        Description = description;
        SetUpdatedBy(updatedBy);

        context.Set<DocumentEntity>().Update(this);
        await context.SaveChangesAsync(cancellationToken);
    }

    // Active Record: Delete
    public async Task DeleteAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        context.Set<DocumentEntity>().Remove(this);
        await context.SaveChangesAsync(cancellationToken);
    }

    // Active Record: Find by ID
    public static async Task<DocumentEntity?> FindByIdAsync(DbContext context, Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Set<DocumentEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    // Active Record: Get all
    public static async Task<List<DocumentEntity>> GetAllAsync(DbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<DocumentEntity>()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}