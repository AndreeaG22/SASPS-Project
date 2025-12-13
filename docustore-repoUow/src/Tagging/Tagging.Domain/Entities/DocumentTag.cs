namespace Tagging.Domain.Entities;

/// <summary>
/// DocumentTag entity - persistence ignorant.
/// Represents the many-to-many relationship between Documents and Tags.
/// </summary>
public class DocumentTag
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DocumentId { get; private set; }
    public Guid TagId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // Navigation property
    public Tag Tag { get; private set; } = null!;

    // EF Core requires a parameterless constructor
    private DocumentTag() { }

    public static DocumentTag Create(Guid documentId, Guid tagId, string createdBy)
    {
        var documentTag = new DocumentTag
        {
            DocumentId = documentId,
            TagId = tagId,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        return documentTag;
    }
}
