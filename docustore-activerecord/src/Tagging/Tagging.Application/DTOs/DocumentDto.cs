namespace Tagging.Application.DTOs;

public record DocumentDto(
    Guid Id,
    string Title,
    string? Description,
    string FileName,
    long FileSizeInBytes,
    string ContentType,
    DateTime CreatedAt,
    string CreatedBy,
    string Status
);
