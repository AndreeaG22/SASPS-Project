namespace Versioning.Domain.Entities;

/// <summary>
/// Version entity - persistence ignorant.
/// No direct database access, ORM-specific code, or persistence logic.
/// </summary>
public class VersionEntity
{
    private const long MaxFileSizeInBytes = 20 * 1024 * 1024; // 20MB

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DocumentId { get; private set; }
    public int VersionNumber { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string FilePathOnDisk { get; private set; } = string.Empty;
    public long FileSizeInBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public string? Notes { get; private set; }
    public bool IsCurrent { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // EF Core requires a parameterless constructor
    private VersionEntity()
    {
    }

    public static VersionEntity Create(
        Guid documentId,
        int versionNumber,
        string fileName,
        string contentType,
        string? notes,
        string createdBy)
    {
        // Validation
        if (documentId == Guid.Empty)
            throw new InvalidOperationException("Document ID is required");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new InvalidOperationException("File name is required");

        if (notes?.Length > 500)
            throw new InvalidOperationException("Notes cannot exceed 500 characters");

        var version = new VersionEntity
        {
            DocumentId = documentId,
            VersionNumber = versionNumber,
            FileName = fileName,
            ContentType = contentType,
            Notes = notes,
            IsCurrent = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        return version;
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

    public void SetAsCurrent(string userId)
    {
        IsCurrent = true;
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsNotCurrent()
    {
        IsCurrent = false;
    }
}
