using MediatR;
using Tagging.Application.DTOs;

namespace Tagging.Application.Queries.ListTags;

public record ListTagsQuery(
    bool IncludeDocumentCount = false
) : IRequest<List<TagDto>>;
