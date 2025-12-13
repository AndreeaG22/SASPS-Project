using Document.Application.DTOs;
using Document.Application.Interfaces;
using MediatR;

namespace Document.Application.Queries.ListDocuments;

public class ListDocumentsQueryHandler : IRequestHandler<ListDocumentsQuery, ListDocumentsResult>
{
    private readonly IDocumentRepository _documentRepository;

    public ListDocumentsQueryHandler(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<ListDocumentsResult> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _documentRepository.GetAllActiveAsync(cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            return new ListDocumentsResult(new List<DocumentDto>(), 0);
        }

        var documentDtos = documents.Select(document => new DocumentDto(
            Id: document.Id,
            Title: document.Title,
            Description: document.Description,
            FileName: document.FileName,
            FileSizeInBytes: document.FileSizeInBytes,
            ContentType: document.ContentType,
            CreatedAt: document.CreatedAt,
            CreatedBy: document.CreatedBy,
            Status: document.Status
        )).ToList();

        return new ListDocumentsResult(documentDtos, documentDtos.Count);
    }
}
