using Microsoft.EntityFrameworkCore;
using Versioning.Domain.Common;
using Versioning.Domain.Services;

namespace Versioning.Domain.Entities;

public class VersionEntity : ActiveRecordBase<VersionEntity>
{
    private const long MaxFileSizeInBytes = 20 * 1024 * 1024; // 20MB

    public Guid DocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePathOnDisk { get; private set; } = string.Empty;
    public long FileSizeInBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public bool IsCurrent { get; private set; }

    // EF Core requires a parameterless constructor
    private VersionEntity()
    {
    }

    public static async Task<VersionEntity> Create(
        Guid documentId,
        string fileName,
        string contentType,
        string? notes,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // Validation
        if (documentId == Guid.Empty)
            throw new InvalidOperationException("Document ID is required");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("File name is required");

        if (notes?.Length > 500)
            throw new InvalidOperationException("Notes cannot exceed 500 characters");

        // Check if document exists and is not deleted
        var documentExists = await CheckDocumentExists(documentId, cancellationToken);
        if (!documentExists)
            throw new InvalidOperationException(
                $"Cannot add version. Document with ID '{documentId}' not found or is deleted");

        var nextVersionNumber = await GetNextVersionNumber(documentId, cancellationToken);

        var version = new VersionEntity
        {
            DocumentId = documentId,
            VersionNumber = nextVersionNumber,
            FileName = fileName,
            ContentType = contentType,
            Notes = notes,
            IsCurrent = true
        };

        version.SetCreatedBy(createdBy);
        return version;
    }

    public async Task UploadAndSave(byte[] fileContent, CancellationToken cancellationToken = default)
    {
        if (fileContent.Length > MaxFileSizeInBytes)
        {
            throw new InvalidOperationException(
                $"File size exceeds the maximum allowed size of {MaxFileSizeInBytes / 1024 / 1024}MB");
        }

        var fileStorageService = GetService<IFileStorageService>();

        FilePathOnDisk = await fileStorageService.SaveVersionFileAsync(
            DocumentId,
            fileContent,
            VersionNumber,
            cancellationToken);

        FileSizeInBytes = fileContent.Length;

        await MarkOtherVersionsAsNotCurrent(cancellationToken);

        await Save(cancellationToken);

        await fileStorageService.UpdateCurrentVersionMarkerAsync(
            DocumentId,
            VersionNumber,
            cancellationToken);
    }

    private async Task MarkOtherVersionsAsNotCurrent(CancellationToken cancellationToken)
    {
        var context = GetDbContext();
        var currentVersions = await context.Set<VersionEntity>()
            .Where(v => v.DocumentId == this.DocumentId && v.Id != this.Id && v.IsCurrent)
            .ToListAsync(cancellationToken);

        foreach (var version in currentVersions)
        {
            version.IsCurrent = false;
        }
    }

    public async Task Save(CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        var dbSet = context.Set<VersionEntity>();

        var exists = await dbSet.AnyAsync(v => v.Id == this.Id, cancellationToken);

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

    public async Task<byte[]> GetFileContent(CancellationToken cancellationToken = default)
    {
        var fileStorageService = GetService<IFileStorageService>();
        return await fileStorageService.GetVersionFileAsync(DocumentId, VersionNumber, cancellationToken);
    }

    public static async Task<VersionEntity?> GetVersionByNumber(
        Guid documentId,
        int versionNumber,
        CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<VersionEntity>()
            .Where(v => v.DocumentId == documentId && v.VersionNumber == versionNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static async Task<bool> CheckDocumentExists(Guid documentId, CancellationToken cancellationToken)
    {
        try
        {
            var document = await Document.Domain.Entities.DocumentEntity.Find(documentId, cancellationToken);
            return document != null && document.Status == Document.Domain.Enums.DocumentStatus.Active;
        }
        catch
        {
            // If we can't verify, fail safely by not allowing the version
            return false;
        }
    }

    public async Task SetAsCurrent(string userId, CancellationToken cancellationToken = default)
    {
        // Get the current version first
        var context = GetDbContext();
        var currentVersion = await context.Set<VersionEntity>()
            .Where(v => v.DocumentId == this.DocumentId && v.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentVersion?.Id == this.Id)
        {
            return;
        }

        var previousCurrentVersionNumber = currentVersion?.VersionNumber ?? this.VersionNumber;

        var allVersions = await context.Set<VersionEntity>()
            .Where(v => v.DocumentId == this.DocumentId && v.IsCurrent)
            .ToListAsync(cancellationToken);

        foreach (var version in allVersions)
        {
            version.IsCurrent = false;
            version.SetUpdatedBy(userId);
        }

        this.IsCurrent = true;
        this.SetUpdatedBy(userId);

        await context.SaveChangesAsync(cancellationToken);

        var fileStorageService = GetService<IFileStorageService>();
        await fileStorageService.UpdateCurrentVersionMarkerAsync(
            DocumentId,
            VersionNumber,
            cancellationToken);

        if (previousCurrentVersionNumber != this.VersionNumber)
        {
            var eventPublisher = GetService<Shared.Events.IEventPublisher>();
            var versionChangedEvent = new Shared.Events.VersionChangedEvent(
                DocumentId: this.DocumentId,
                NewCurrentVersionNumber: this.VersionNumber,
                PreviousCurrentVersionNumber: previousCurrentVersionNumber,
                ChangedBy: userId,
                ChangedAt: DateTime.UtcNow
            );

            await eventPublisher.PublishAsync(versionChangedEvent, cancellationToken);
        }
    }

    private static async Task<int> GetNextVersionNumber(Guid documentId, CancellationToken cancellationToken)
    {
        var context = GetDbContext();
        var maxVersion = await context.Set<VersionEntity>()
            .Where(v => v.DocumentId == documentId)
            .MaxAsync(v => (int?)v.VersionNumber, cancellationToken);

        return (maxVersion ?? 0) + 1;
    }

    public static async Task<VersionEntity?> Find(Guid id, CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<VersionEntity>().FindAsync(new object[] { id }, cancellationToken);
    }

    public static async Task<List<VersionEntity>> GetDocumentVersions(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<VersionEntity>()
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public static async Task<VersionEntity?> GetCurrentVersion(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<VersionEntity>()
            .Where(v => v.DocumentId == documentId && v.IsCurrent)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<int> Count(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var context = GetDbContext();
        return await context.Set<VersionEntity>()
            .CountAsync(v => v.DocumentId == documentId, cancellationToken);
    }
}