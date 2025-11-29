using MediatR;
using Tagging.Application.DTOs;

namespace Tagging.Application.Queries.GetDocumentTags;

public record GetDocumentTagsQuery(
    Guid DocumentId
) : IRequest<List<DocumentTagDto>>;
