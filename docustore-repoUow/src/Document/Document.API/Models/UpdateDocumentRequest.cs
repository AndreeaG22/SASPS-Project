namespace Document.API.Models;

public record UpdateDocumentRequest(
    string Title,
    string? Description
);