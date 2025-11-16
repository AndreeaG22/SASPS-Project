namespace Document.API.Models;

public record ListDocumentsResponse(
    IReadOnlyList<DocumentResponse> Documents,
    int TotalCount);