using MediatR;
using Tagging.Application.DTOs;
using Tagging.Application.Interfaces;

namespace Tagging.Application.Queries.ListTags;

public class ListTagsQueryHandler : IRequestHandler<ListTagsQuery, List<TagDto>>
{
    private readonly ITagRepository _tagRepository;
    private readonly IDocumentTagRepository _documentTagRepository;

    public ListTagsQueryHandler(
        ITagRepository tagRepository,
        IDocumentTagRepository documentTagRepository)
    {
        _tagRepository = tagRepository;
        _documentTagRepository = documentTagRepository;
    }

    public async Task<List<TagDto>> Handle(ListTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);

        Dictionary<Guid, int>? documentCounts = null;
        if (request.IncludeDocumentCount)
        {
            documentCounts = await _documentTagRepository.GetDocumentCountsByTagAsync(cancellationToken);
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
