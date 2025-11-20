namespace Versioning.Application.DTOs;

public record VersionDownloadDto(
    Guid Id,
    Guid DocumentId,
    int VersionNumber,
    string FileName,
    byte[] FileContent,
    string ContentType,
    long FileSizeInBytes
);