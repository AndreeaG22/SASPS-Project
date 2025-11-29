// Versioning/Versioning.Application/EventHandlers/DocumentCreatedEventHandler.cs

using Microsoft.Extensions.Logging;
using Shared.Events;
using Versioning.Domain.Entities;

namespace Versioning.Application.EventHandlers;

public class DocumentCreatedEventHandler(ILogger<DocumentCreatedEventHandler> logger)
    : IEventHandler<DocumentCreatedEvent>
{
    public async Task HandleAsync(DocumentCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "Creating initial version for document {DocumentId}", 
                @event.DocumentId);

            var version = await VersionEntity.Create(
                documentId: @event.DocumentId,
                fileName: @event.FileName,
                contentType: @event.ContentType,
                notes: "Initial version",
                createdBy: @event.CreatedBy,
                cancellationToken: cancellationToken
            );

            await version.UploadAndSave(@event.FileContent, cancellationToken);

            logger.LogInformation(
                "Successfully created version 1 for document {DocumentId}", 
                @event.DocumentId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex, 
                "Failed to create initial version for document {DocumentId}", 
                @event.DocumentId);
            // Don't throw - we don't want to fail document creation if versioning fails
        }
    }
}