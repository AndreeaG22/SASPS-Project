using MediatR;
using Tagging.Application.DTOs;
using Tagging.Domain.Entities;

namespace Tagging.Application.Queries.GetDocumentTags;

public class GetDocumentTagsQueryHandler : IRequestHandler<GetDocumentTagsQuery, List<DocumentTagDto>>
{
    public async Task<List<DocumentTagDto>> Handle(GetDocumentTagsQuery request, CancellationToken cancellationToken)
    {
        var documentTags = await DocumentTag.GetByDocument(request.DocumentId, cancellationToken);

        return documentTags.Select(dt => new DocumentTagDto(
            Id: dt.Id,
            TagId: dt.TagId,
            TagName: dt.Tag.Name,
            TagDescription: dt.Tag.Description,
            AddedAt: dt.CreatedAt,
            AddedBy: dt.CreatedBy
        )).ToList();
    }
}
