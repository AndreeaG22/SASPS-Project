namespace Document.API.Models;

public record CreateDocumentRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IFormFile File { get; init; } = null!;
}