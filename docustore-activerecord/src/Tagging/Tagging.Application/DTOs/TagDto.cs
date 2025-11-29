namespace Tagging.Application.DTOs;

public record TagDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    string CreatedBy,
    int? DocumentCount = null
);
