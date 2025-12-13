using Microsoft.Extensions.Logging;
using Shared.Events;
using Versioning.Application.Interfaces;
using Versioning.Domain.Entities;
using Versioning.Domain.Services;

namespace Versioning.Application.EventHandlers;

public class DocumentCreatedEventHandler : IEventHandler<DocumentCreatedEvent>
{
    private readonly IVersioningUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DocumentCreatedEventHandler> _logger;

    public DocumentCreatedEventHandler(
        IVersioningUnitOfWork unitOfWork,
        IFileStorageService fileStorageService,
        ILogger<DocumentCreatedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task HandleAsync(DocumentCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Creating initial version for document {DocumentId}",
                @event.DocumentId);

            // Get next version number (should be 1 for new document)
            var nextVersionNumber = await _unitOfWork.Versions.GetNextVersionNumberAsync(@event.DocumentId, cancellationToken);

            // Create version entity
            var version = VersionEntity.Create(
                documentId: @event.DocumentId,
                versionNumber: nextVersionNumber,
                fileName: @event.FileName,
                contentType: @event.ContentType,
                notes: "Initial version",
                createdBy: @event.CreatedBy
            );

            // Save file to disk
            var filePathOnDisk = await _fileStorageService.SaveVersionFileAsync(
                @event.DocumentId,
                @event.FileContent,
                nextVersionNumber,
                cancellationToken);

            // Set file info
            version.SetFileInfo(filePathOnDisk, @event.FileContent.Length);

            // Add version and save
            await _unitOfWork.Versions.AddAsync(version, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Update current version marker on disk
            await _fileStorageService.UpdateCurrentVersionMarkerAsync(
                @event.DocumentId,
                nextVersionNumber,
                cancellationToken);

            _logger.LogInformation(
                "Successfully created version 1 for document {DocumentId}",
                @event.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create initial version for document {DocumentId}",
                @event.DocumentId);
            // Don't throw - we don't want to fail document creation if versioning fails
        }
    }
}
