namespace Shared.Events;

public record VersionChangedEvent(
    Guid DocumentId,
    int NewCurrentVersionNumber,
    int PreviousCurrentVersionNumber,
    string ChangedBy,
    DateTime ChangedAt
);