using Document.Domain.Enums;

namespace Document.Domain.Entities;

/// <summary>
/// Document entity - persistence ignorant.
/// No direct database access, ORM-specific code, or persistence logic.
/// </summary>
public class DocumentEntity
{
    private const long MaxFileSizeInBytes = 20 * 1024 * 1024; // 20MB

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePathOnDisk { get; private set; } = string.Empty;
    public long FileSizeInBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public DocumentStatus Status { get; private set; } = DocumentStatus.Active;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
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
            Status = DocumentStatus.Active,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        return document;
    }

    public void SetFileInfo(string filePathOnDisk, long fileSizeInBytes)
    {
        if (fileSizeInBytes > MaxFileSizeInBytes)
        {
            throw new InvalidOperationException(
                $"File size exceeds the maximum allowed size of {MaxFileSizeInBytes / 1024 / 1024}MB");
        }

        FilePathOnDisk = filePathOnDisk;
        FileSizeInBytes = fileSizeInBytes;
    }

    public void Update(string title, string? description, string updatedBy)
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
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete(string deletedBy)
    {
        if (Status == DocumentStatus.Deleted)
        {
            throw new InvalidOperationException("Document is already deleted");
        }

        Status = DocumentStatus.Deleted;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        UpdatedBy = deletedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore(string restoredBy)
    {
        if (Status != DocumentStatus.Deleted)
        {
            throw new InvalidOperationException("Only deleted documents can be restored");
        }

        Status = DocumentStatus.Active;
        DeletedAt = null;
        DeletedBy = null;
        UpdatedBy = restoredBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
