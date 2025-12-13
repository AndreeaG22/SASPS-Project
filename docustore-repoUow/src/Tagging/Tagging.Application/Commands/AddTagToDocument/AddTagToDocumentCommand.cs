using MediatR;
using Tagging.Application.DTOs;

namespace Tagging.Application.Commands.AddTagToDocument;

public record AddTagToDocumentCommand(
    Guid DocumentId,
    Guid TagId,
    string UserId
) : IRequest<DocumentTagDto>;
