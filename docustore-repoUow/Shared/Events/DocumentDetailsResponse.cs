namespace Shared.Events;

public record DocumentDetailsResponse(
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
