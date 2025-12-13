using MediatR;
using Tagging.Application.DTOs;
using Tagging.Application.Interfaces;
using Document.Application.Queries.GetDocument;
using Document.Domain.Enums;

namespace Tagging.Application.Queries.GetDocumentsByTag;

public class GetDocumentsByTagQueryHandler : IRequestHandler<GetDocumentsByTagQuery, List<DocumentDto>>
{
    private readonly IMediator _mediator;
    private readonly ITagRepository _tagRepository;
    private readonly IDocumentTagRepository _documentTagRepository;

    public GetDocumentsByTagQueryHandler(
        IMediator mediator,
        ITagRepository tagRepository,
        IDocumentTagRepository documentTagRepository)
    {
        _mediator = mediator;
        _tagRepository = tagRepository;
        _documentTagRepository = documentTagRepository;
    }

    public async Task<List<DocumentDto>> Handle(GetDocumentsByTagQuery request, CancellationToken cancellationToken)
    {
        // Validate tag exists
        var tagExists = await _tagRepository.ExistsAsync(request.TagId, cancellationToken);
        if (!tagExists)
        {
            throw new InvalidOperationException($"Tag with ID '{request.TagId}' not found");
        }

        var documentTags = await _documentTagRepository.GetByTagIdAsync(request.TagId, cancellationToken);
        var documentIds = documentTags.Select(dt => dt.DocumentId).Distinct().ToList();

        // Fetch document details from Document module using MediatR
        var documents = new List<DocumentDto>();
        foreach (var docId in documentIds)
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
