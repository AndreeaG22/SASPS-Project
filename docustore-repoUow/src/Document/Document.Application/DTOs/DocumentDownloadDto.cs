namespace Document.Application.DTOs;

public record DocumentDownloadDto(
    Guid DocumentId,
    int VersionNumber,
    string FileName,
    byte[] FileContent,
    string ContentType,
    long FileSizeInBytes
);
