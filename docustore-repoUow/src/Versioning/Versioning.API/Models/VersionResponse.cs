namespace Versioning.API.Models;

public record VersionResponse(
    Guid Id,
    Guid DocumentId,
    int VersionNumber,
    string FileName,
    long FileSizeInBytes,
    string ContentType,
    string? Notes,
    bool IsCurrent,
    DateTime CreatedAt,
    string CreatedBy
);