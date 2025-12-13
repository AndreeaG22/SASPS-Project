namespace Versioning.Application.DTOs;

public record VersionHistoryResult(
    Guid DocumentId,
    IReadOnlyList<VersionDto> Versions,
    int TotalCount
);