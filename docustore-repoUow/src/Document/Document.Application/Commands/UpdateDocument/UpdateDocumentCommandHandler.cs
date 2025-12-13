using Document.Application.DTOs;
using Document.Application.Interfaces;
using MediatR;

namespace Document.Application.Commands.UpdateDocument;

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, DocumentDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateDocumentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DocumentDto> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(request.Id, cancellationToken);

        if (document == null)
        {
            throw new InvalidOperationException($"Document with ID '{request.Id}' not found");
        }

        // Update domain entity (business logic in domain)
        document.Update(request.Title, request.Description, request.UserId);

        // Update via repository and save
        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
