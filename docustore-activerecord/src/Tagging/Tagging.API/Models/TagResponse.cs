namespace Tagging.API.Models;

public record TagResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    string CreatedBy,
    int? DocumentCount = null
);
