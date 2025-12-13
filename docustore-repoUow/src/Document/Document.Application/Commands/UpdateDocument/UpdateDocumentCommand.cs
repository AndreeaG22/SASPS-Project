using Document.Application.DTOs;
using MediatR;

namespace Document.Application.Commands.UpdateDocument;

public record UpdateDocumentCommand(
    Guid Id,
    string Title,
    string? Description,
    string UserId
) : IRequest<DocumentDto>;
