namespace Shared.Events;

public record DocumentCreatedEvent(
    Guid DocumentId,
    string Title,
    string? Description,
    string FileName,
    byte[] FileContent,
    string ContentType,
    string CreatedBy,
    DateTime CreatedAt
);