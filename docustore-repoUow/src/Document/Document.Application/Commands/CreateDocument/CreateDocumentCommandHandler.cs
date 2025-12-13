using Document.Application.DTOs;
using Document.Application.Interfaces;
using Document.Domain.Entities;
using Document.Domain.Services;
using MediatR;
using Shared.Events;

namespace Document.Application.Commands.CreateDocument;

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly IEventPublisher _eventPublisher;

    public CreateDocumentCommandHandler(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _eventPublisher = eventPublisher;
    }

    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        // Create the document entity (business logic in domain)
        var document = DocumentEntity.Create(
            title: request.Title,
            description: request.Description,
            fileName: request.FileName,
            contentType: request.ContentType,
            createdBy: request.UserId
        );

        // Create document folder on disk
        var filePathOnDisk = await _fileStorageService.CreateDocumentFolderAsync(
            document.Id,
            document.FileName,
            cancellationToken);

        // Set file info on the document entity
        document.SetFileInfo(filePathOnDisk, request.FileContent.Length);

        // Add to repository and save
        await _unitOfWork.Documents.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event - versioning module will handle saving the file
        var documentCreatedEvent = new DocumentCreatedEvent(
            DocumentId: document.Id,
            Title: document.Title,
            Description: document.Description,
            FileName: document.FileName,
            FileContent: request.FileContent,
            ContentType: document.ContentType,
            CreatedBy: document.CreatedBy,
            CreatedAt: document.CreatedAt
        );

        await _eventPublisher.PublishAsync(documentCreatedEvent, cancellationToken);

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
