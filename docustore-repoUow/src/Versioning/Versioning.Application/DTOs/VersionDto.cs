namespace Versioning.Application.DTOs;

public record VersionDto(
    Guid Id,
    Guid DocumentId,
    int VersionNumber,
    string FileName,
    long FileSizeInBytes,
    string ContentType,
    string? Notes,
    bool IsCurrent,
    DateTime CreatedAt,
    string CreatedBy);