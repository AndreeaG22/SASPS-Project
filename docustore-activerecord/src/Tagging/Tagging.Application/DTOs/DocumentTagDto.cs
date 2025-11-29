namespace Tagging.Application.DTOs;

public record DocumentTagDto(
    Guid Id,
    Guid TagId,
    string TagName,
    string? TagDescription,
    DateTime AddedAt,
    string AddedBy
);
