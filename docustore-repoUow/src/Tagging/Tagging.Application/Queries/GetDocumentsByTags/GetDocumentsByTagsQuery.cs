using MediatR;
using Tagging.Application.DTOs;

namespace Tagging.Application.Queries.GetDocumentsByTags;

public record GetDocumentsByTagsQuery(
    List<Guid> TagIds
) : IRequest<List<DocumentDto>>;
