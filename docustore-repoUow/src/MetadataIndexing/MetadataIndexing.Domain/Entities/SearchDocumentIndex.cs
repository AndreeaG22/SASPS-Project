namespace MetadataIndexing.Domain.Entities;

/// <summary>
/// SearchDocumentIndex entity - persistence ignorant.
/// Used for indexing document metadata for search purposes.
/// </summary>
public class SearchDocumentIndex
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DocumentId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeInBytes { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

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
            IsDeleted = false,
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };

        return index;
    }

    public void UpdateMetadata(
        string title,
        string? description,
        string updatedBy)
    {
        Title = title;
        Description = description;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted(string deletedBy)
    {
        IsDeleted = true;
        UpdatedBy = deletedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
