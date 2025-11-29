namespace MetadataIndexing.Application.DTOs;

public record SearchDocumentDto(
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
