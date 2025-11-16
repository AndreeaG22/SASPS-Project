using Document.Application.DTOs;
using Document.Domain.Entities;
using MediatR;

namespace Document.Application.Commands.CreateDocument;

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = DocumentEntity.Create(
            title: request.Title,
            description: request.Description,
            fileName: request.FileName,
            contentType: request.ContentType,
            createdBy: request.UserId
        );

        await document.UploadAndSave(request.FileContent, cancellationToken);

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