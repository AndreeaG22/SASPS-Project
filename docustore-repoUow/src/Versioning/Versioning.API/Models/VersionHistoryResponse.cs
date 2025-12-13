namespace Versioning.API.Models;

public record VersionHistoryResponse(
    Guid DocumentId,
    IReadOnlyList<VersionResponse> Versions,
    int TotalCount
);