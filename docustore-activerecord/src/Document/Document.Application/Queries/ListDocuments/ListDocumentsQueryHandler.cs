using Document.Application.DTOs;
using Document.Domain.Entities;
using MediatR;

namespace Document.Application.Queries.ListDocuments;

public class ListDocumentsQueryHandler : IRequestHandler<ListDocumentsQuery, ListDocumentsResult>
{
    public async Task<ListDocumentsResult> Handle(ListDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await DocumentEntity.All(cancellationToken);

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
            CreatedBy: document.CreatedBy
        )).ToList();
        
        return new ListDocumentsResult(documentDtos, documentDtos.Count);
    }
}