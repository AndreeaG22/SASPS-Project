using MediatR;
using Tagging.Application.DTOs;
using Tagging.Domain.Entities;
using Document.Application.Queries.GetDocument;
using Document.Domain.Enums;

namespace Tagging.Application.Queries.GetDocumentsByTags;

public class GetDocumentsByTagsQueryHandler : IRequestHandler<GetDocumentsByTagsQuery, List<DocumentDto>>
{
    private readonly IMediator _mediator;

    public GetDocumentsByTagsQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<List<DocumentDto>> Handle(GetDocumentsByTagsQuery request, CancellationToken cancellationToken)
    {
        if (request.TagIds == null || request.TagIds.Count == 0)
        {
            throw new ArgumentException("At least one tag ID must be provided");
        }

        // Get all document-tag associations for the specified tags
        var documentTags = await DocumentTag.GetByTags(request.TagIds, cancellationToken);

        // Group by document ID and filter for documents that have ALL specified tags
        var documentIdsWithAllTags = documentTags
            .GroupBy(dt => dt.DocumentId)
            .Where(g => g.Select(dt => dt.TagId).Distinct().Count() == request.TagIds.Count)
            .Select(g => g.Key)
            .ToList();

        // Fetch document details from Document module using MediatR
        var documents = new List<DocumentDto>();
        foreach (var docId in documentIdsWithAllTags)
        {
            var document = await _mediator.Send(new GetDocumentQuery(docId), cancellationToken);
            if (document != null && document.Status != DocumentStatus.Deleted)
            {
                documents.Add(new DocumentDto(
                    Id: document.Id,
                    Title: document.Title,
                    Description: document.Description,
                    FileName: document.FileName,
                    FileSizeInBytes: document.FileSizeInBytes,
                    ContentType: document.ContentType,
                    CreatedAt: document.CreatedAt,
                    CreatedBy: document.CreatedBy,
                    Status: document.Status.ToString()
                ));
            }
        }

        return documents.OrderByDescending(d => d.CreatedAt).ToList();
    }
}
