using MetadataIndexing.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Events;

namespace MetadataIndexing.Infrastructure.EventHandlers;

public class DocumentUpdatedEventHandler : IEventHandler<DocumentUpdatedEvent>
{
    private readonly IMetadataIndexingUnitOfWork _unitOfWork;
    private readonly ILogger<DocumentUpdatedEventHandler> _logger;

    public DocumentUpdatedEventHandler(
        IMetadataIndexingUnitOfWork unitOfWork,
        ILogger<DocumentUpdatedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(DocumentUpdatedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating search index for document {DocumentId}", @event.DocumentId);

            var searchIndex = await _unitOfWork.SearchDocumentIndexes.GetByDocumentIdAsync(@event.DocumentId, cancellationToken);
            if (searchIndex == null)
            {
                _logger.LogWarning("Document {DocumentId} is not indexed, skipping update", @event.DocumentId);
                return;
            }

            searchIndex.UpdateMetadata(@event.Title, @event.Description, @event.UpdatedBy);
            _unitOfWork.SearchDocumentIndexes.Update(searchIndex);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated search index for document {DocumentId}", @event.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update search index for document {DocumentId}", @event.DocumentId);
            throw;
        }
    }
}
