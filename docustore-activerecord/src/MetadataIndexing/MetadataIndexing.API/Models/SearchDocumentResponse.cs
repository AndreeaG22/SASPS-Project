namespace MetadataIndexing.API.Models;

public record SearchDocumentResponse(
    Guid Id,
    Guid DocumentId,
    string Title,
    string? Description,
    string FileName,
    string ContentType,
    long FileSizeInBytes,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);
