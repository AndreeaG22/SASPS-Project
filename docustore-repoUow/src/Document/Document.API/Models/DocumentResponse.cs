namespace Document.API.Models;

public record DocumentResponse(
    Guid Id,
    string Title,
    string? Description,
    string FileName,
    long FileSizeInBytes,
    string ContentType,
    DateTime CreatedAt,
    string CreatedBy,
    string Status,
    DateTime? DeletedAt = null,
    string? DeletedBy = null
);