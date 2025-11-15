using Document.Application.DTOs;
using MediatR;

namespace Document.Application.Commands.CreateDocument;

public record CreateDocumentCommand(
    string Title,
    string? Description,
    string FileName,
    byte[] FileContent,
    string ContentType,
    string UserId
) : IRequest<DocumentDto>;