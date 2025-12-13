using MediatR;

namespace Document.Application.Commands.DeleteDocument;

public record DeleteDocumentCommand(
    Guid Id,
    string UserId
) : IRequest<Unit>;
