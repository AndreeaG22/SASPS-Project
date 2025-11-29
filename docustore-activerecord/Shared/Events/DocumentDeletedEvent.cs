namespace Shared.Events;

public record DocumentDeletedEvent(
    Guid DocumentId,
    string DeletedBy,
    DateTime DeletedAt
);
