using MediatR;
using Tagging.Application.DTOs;
using Tagging.Application.Interfaces;

namespace Tagging.Application.Queries.GetDocumentTags;

public class GetDocumentTagsQueryHandler : IRequestHandler<GetDocumentTagsQuery, List<DocumentTagDto>>
{
    private readonly IDocumentTagRepository _documentTagRepository;
    private readonly ITagRepository _tagRepository;

    public GetDocumentTagsQueryHandler(
        IDocumentTagRepository documentTagRepository,
        ITagRepository tagRepository)
    {
        _documentTagRepository = documentTagRepository;
        _tagRepository = tagRepository;
    }

    public async Task<List<DocumentTagDto>> Handle(GetDocumentTagsQuery request, CancellationToken cancellationToken)
    {
        var documentTags = await _documentTagRepository.GetByDocumentIdAsync(request.DocumentId, cancellationToken);

        var result = new List<DocumentTagDto>();
        foreach (var dt in documentTags)
        {
            var tag = await _tagRepository.GetByIdAsync(dt.TagId, cancellationToken);
            if (tag != null)
            {
                result.Add(new DocumentTagDto(
                    Id: dt.Id,
                    TagId: dt.TagId,
                    TagName: tag.Name,
                    TagDescription: tag.Description,
                    AddedAt: dt.CreatedAt,
                    AddedBy: dt.CreatedBy
                ));
            }
        }

        return result;
    }
}
