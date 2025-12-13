namespace Versioning.API.Models;

public record CreateVersionRequest
{
    public Guid DocumentId { get; init; }
    public string? Notes { get; init; }
    public IFormFile File { get; init; } = null!;
}