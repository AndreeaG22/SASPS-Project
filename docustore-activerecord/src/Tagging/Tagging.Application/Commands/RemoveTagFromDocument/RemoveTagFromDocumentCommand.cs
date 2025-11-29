using MediatR;

namespace Tagging.Application.Commands.RemoveTagFromDocument;

public record RemoveTagFromDocumentCommand(
    Guid DocumentId,
    Guid TagId
) : IRequest;
