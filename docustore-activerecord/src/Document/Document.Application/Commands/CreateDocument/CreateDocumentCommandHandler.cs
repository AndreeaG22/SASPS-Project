using Document.Application.DTOs;
using Document.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Document.Application.Services;

namespace Document.Application.Commands.CreateDocument;

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, DocumentDto>
{
    private const long MaxFileSizeInBytes = 20 * 1024 * 1024; // 20MB
    private readonly DbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public CreateDocumentCommandHandler(
        DbContext context,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<DocumentDto> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        // Validate file size
        if (request.FileContent.Length > MaxFileSizeInBytes)
        {
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {MaxFileSizeInBytes / 1024 / 1024}MB");
        }

        // Validate title
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new InvalidOperationException("Title cannot be empty or whitespace only");
        }

        if (request.Title.Length > 200)
        {
            throw new InvalidOperationException("Title cannot exceed 200 characters");
        }

        // Validate description
        if (request.Description?.Length > 2000)
        {
            throw new InvalidOperationException("Description cannot exceed 2000 characters");
        }

        // Save file to storage
        var filePath = await _fileStorageService.SaveFileAsync(
            request.FileContent,
            request.FileName,
            cancellationToken);

        var document = DocumentEntity.Create(
            title: request.Title,
            description: request.Description,
            fileName: request.FileName,
            filePathOnDisk: filePath,
            fileSizeInBytes: request.FileContent.Length,
            contentType: request.ContentType,
            createdBy: request.UserId
        );

        await document.SaveAsync(_context, cancellationToken);

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