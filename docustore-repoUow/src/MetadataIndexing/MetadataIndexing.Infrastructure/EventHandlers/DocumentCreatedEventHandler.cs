using MetadataIndexing.Application.Interfaces;
using MetadataIndexing.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Events;

namespace MetadataIndexing.Infrastructure.EventHandlers;

public class DocumentCreatedEventHandler : IEventHandler<DocumentCreatedEvent>
{
    private readonly IMetadataIndexingUnitOfWork _unitOfWork;
    private readonly ILogger<DocumentCreatedEventHandler> _logger;

    public DocumentCreatedEventHandler(
        IMetadataIndexingUnitOfWork unitOfWork,
        ILogger<DocumentCreatedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(DocumentCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Indexing document {DocumentId} for search", @event.DocumentId);

            // Check if document is already indexed
            var existingIndex = await _unitOfWork.SearchDocumentIndexes.GetByDocumentIdAsync(@event.DocumentId, cancellationToken);
            if (existingIndex != null)
            {
                _logger.LogWarning("Document {DocumentId} is already indexed", @event.DocumentId);
                return;
            }

            // Create search index entry
            var searchIndex = SearchDocumentIndex.Create(
                documentId: @event.DocumentId,
                title: @event.Title,
                description: @event.Description,
                fileName: @event.FileName,
                contentType: @event.ContentType,
                fileSizeInBytes: @event.FileContent.Length,
                createdBy: @event.CreatedBy,
                createdAt: @event.CreatedAt
            );

            await _unitOfWork.SearchDocumentIndexes.AddAsync(searchIndex, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully indexed document {DocumentId}", @event.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to index document {DocumentId}", @event.DocumentId);
            throw;
        }
    }
}
