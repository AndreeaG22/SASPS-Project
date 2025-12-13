namespace Document.Application.DTOs;

public record CreateDocumentDto(
    string Title,
    string? Description,
    string FileName,
    byte[] FileContent,
    string ContentType
);