namespace Tagging.API.Models;

public record CreateTagRequest(
    string Name,
    string? Description
);
