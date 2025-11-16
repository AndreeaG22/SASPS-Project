using Document.Application.DTOs;
using Document.Domain.Entities;
using MediatR;

namespace Document.Application.Queries.GetDocument;

public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, DocumentDto?>
{
    public async Task<DocumentDto?> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await DocumentEntity.Find(request.Id, cancellationToken);

        if (document == null)
            return null;

        return new DocumentDto(
            Id: document.Id,
            Title: document.Title,
            Description: document.Description,
            FileName: document.FileName,
            FileSizeInBytes: document.FileSizeInBytes,
            ContentType: document.ContentType,
            CreatedAt: document.CreatedAt,
            CreatedBy: document.CreatedBy
        );
    }
}