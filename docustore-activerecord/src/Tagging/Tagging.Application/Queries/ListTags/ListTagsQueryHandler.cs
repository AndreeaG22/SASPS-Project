using MediatR;
using Tagging.Application.DTOs;
using Tagging.Domain.Entities;

namespace Tagging.Application.Queries.ListTags;

public class ListTagsQueryHandler : IRequestHandler<ListTagsQuery, List<TagDto>>
{
    public async Task<List<TagDto>> Handle(ListTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await Tag.All(cancellationToken);
        
        Dictionary<Guid, int>? documentCounts = null;
        if (request.IncludeDocumentCount)
        {
            documentCounts = await DocumentTag.GetDocumentCountsByTag(cancellationToken);
        }

        return tags.Select(tag => new TagDto(
            Id: tag.Id,
            Name: tag.Name,
            Description: tag.Description,
            CreatedAt: tag.CreatedAt,
            CreatedBy: tag.CreatedBy,
            DocumentCount: request.IncludeDocumentCount && documentCounts != null
                ? documentCounts.GetValueOrDefault(tag.Id, 0)
                : null
        )).ToList();
    }
}
