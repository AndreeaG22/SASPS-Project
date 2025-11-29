using Document.Domain.Common;
using Document.Domain.Enums;
using Document.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Shared.Events;

namespace Document.Domain.Entities;

public class DocumentEntity : ActiveRecordBase
{
    private const long MaxFileSizeInBytes = 20 * 1024 * 1024; // 20MB

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePathOnDisk { get; private set; } = string.Empty;
    public long FileSizeInBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public DocumentStatus Status { get; private set; } = DocumentStatus.Active; // am adadaugat status
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }
    

    // EF Core requires a parameterless constructor
    private DocumentEntity() { }

    public static DocumentEntity Create(
        string title,
        string? description,
        string fileName,
        string contentType,
        string createdBy)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException("Title cannot be empty or whitespace only");

        if (title.Length > 200)
            throw new InvalidOperationException("Title cannot exceed 200 characters");

        if (description?.Length > 2000)
            throw new InvalidOperationException("Description cannot exceed 2000 characters");

        var document = new DocumentEntity
        {
            Title = title,
            Description = description,
            FileName = fileName,
            ContentType = contentType,
            Status = DocumentStatus.Active
        };

        document.SetCreatedBy(createdBy);
        return document;
    }

    public async Task UploadAndSave(byte[] fileContent, CancellationToken cancellationToken = default)
    {
        // Validate file size
        if (fileContent.Length > MaxFileSizeInBytes)
        {
            throw new InvalidOperationException(
                $"File size exceeds the maximum allowed size of {MaxFileSizeInBytes / 1024 / 1024}MB");
        }

        var fileStorageService = GetService<IFileStorageService>();

        // Create document folder
        FilePathOnDisk = await fileStorageService.CreateDocumentFolderAsync(
            this.Id,
            this.FileName,
            cancellationToken);

        FileSizeInBytes = fileContent.Length;

        // Save entity to database first
        await Save(cancellationToken);
    
        // Publish event - versioning module will handle saving the file
        var eventPublisher = GetService<IEventPublisher>();
        var documentCreatedEvent = new DocumentCreatedEvent(
            DocumentId: this.Id,
            Title: this.Title,
            Description: this.Description,
            FileName: this.FileName,
            FileContent: fileContent,
            ContentType: this.ContentType,
            CreatedBy: this.CreatedBy,
            CreatedAt: this.CreatedAt
        );

        await eventPublisher.PublishAsync(documentCreatedEvent, cancellationToken);
    }

    public async Task Save(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        var dbSet = context.Set<DocumentEntity>();

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

    public async Task Update(string title, string? description, string updatedBy, CancellationToken cancellationToken = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidOperationException("Title cannot be empty or whitespace only");

        if (title.Length > 200)
            throw new InvalidOperationException("Title cannot exceed 200 characters");

        if (description?.Length > 2000)
            throw new InvalidOperationException("Description cannot exceed 2000 characters");

        Title = title;
        Description = description;
        SetUpdatedBy(updatedBy);

        await Save(cancellationToken);
    }
    
    public async Task SoftDelete(string deletedBy, CancellationToken cancellationToken = default)
    { 
        if (Status == DocumentStatus.Deleted)
        {
            throw new InvalidOperationException("Document is already deleted");
        }
        
        Status = DocumentStatus.Deleted;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        SetUpdatedBy(deletedBy);

        await Save(cancellationToken);
    }
    
    public async Task Restore(string restoredBy, CancellationToken cancellationToken = default)
    {
        if (Status != DocumentStatus.Deleted)
        {
            throw new InvalidOperationException("Only deleted documents can be restored");
        }

        Status = DocumentStatus.Active;
        DeletedAt = null;
        DeletedBy = null;
        SetUpdatedBy(restoredBy);

        await Save(cancellationToken);
    }

    public async Task HardDelete(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();

        // Delete entire document folder
        var fileStorageService = GetService<IFileStorageService>();
        await fileStorageService.DeleteDocumentFolderAsync(this.Id, cancellationToken);

        // Delete from database
        context.Set<DocumentEntity>().Remove(this);
        await context.SaveChangesAsync(cancellationToken);
    }
    
    public static async Task<DocumentEntity?> Find(Guid id, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    public static async Task<List<DocumentEntity>> All(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentEntity>()
            .Where(d => d.Status == DocumentStatus.Active)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public static async Task<List<DocumentEntity>> AllIncludingDeleted(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentEntity>()
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public static async Task<List<DocumentEntity>> Where(
        Func<DocumentEntity, bool> predicate,
        CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        var allDocuments = await context.Set<DocumentEntity>()
            .Where(d => d.Status == DocumentStatus.Active)
            .ToListAsync(cancellationToken);
        return allDocuments.Where(predicate).ToList();
    }

    public static async Task<int> Count(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentEntity>()
            .CountAsync(d => d.Status == DocumentStatus.Active, cancellationToken);
    }

    public static async Task<bool> Exists(Guid id, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<DocumentEntity>().AnyAsync(d => d.Id == id, cancellationToken);
    }

    public static async Task<int> DeleteAll(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        var documents = await context.Set<DocumentEntity>().ToListAsync(cancellationToken);
        context.Set<DocumentEntity>().RemoveRange(documents);
        return await context.SaveChangesAsync(cancellationToken);
    }
}