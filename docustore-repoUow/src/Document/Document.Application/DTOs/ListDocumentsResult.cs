namespace Document.Application.DTOs;

public record ListDocumentsResult(
    IReadOnlyList<DocumentDto> Documents,
    int TotalCount);