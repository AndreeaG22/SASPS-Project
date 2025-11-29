namespace Tagging.API.Models;

public record DocumentTagResponse(
    Guid Id,
    Guid TagId,
    string TagName,
    string? TagDescription,
    DateTime AddedAt,
    string AddedBy
);
