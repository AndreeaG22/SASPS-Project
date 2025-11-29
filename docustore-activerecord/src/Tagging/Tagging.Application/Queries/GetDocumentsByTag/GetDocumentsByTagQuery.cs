using MediatR;
using Tagging.Application.DTOs;

namespace Tagging.Application.Queries.GetDocumentsByTag;

public record GetDocumentsByTagQuery(
    Guid TagId
) : IRequest<List<DocumentDto>>;
