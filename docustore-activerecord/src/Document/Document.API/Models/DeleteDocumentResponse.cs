namespace Document.API.Models;

public record DeleteDocumentResponse(
    Guid Id,
    string Message,
    DateTime DeletedAt
);