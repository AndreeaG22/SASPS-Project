namespace Shared.Events;

public record DocumentUpdatedEvent(
    Guid DocumentId,
    string Title,
    string? Description,
    string UpdatedBy,
    DateTime UpdatedAt
);
