using Document.Application.DTOs;
using Document.Domain.Entities;
using MediatR;

namespace Document.Application.Commands.UpdateDocument;

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, DocumentDto> // aici prima entitate este ce intra si a doua e ce iese. cand facem .send() in API o sa ajunga aici 
{
    public async Task<DocumentDto> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await DocumentEntity.Find(request.Id, cancellationToken);

        if (document == null)
        {
            throw new InvalidOperationException($"Document with ID '{request.Id}' not found");
        }

        await document.Update(request.Title, request.Description, request.UserId, cancellationToken);

        return new DocumentDto(
            Id: document.Id,
            Title: document.Title,
            Description: document.Description,
            FileName: document.FileName,
            FileSizeInBytes: document.FileSizeInBytes,
            ContentType: document.ContentType,
            CreatedAt: document.CreatedAt,
            CreatedBy: document.CreatedBy,
            Status: document.Status
        );
    }
}