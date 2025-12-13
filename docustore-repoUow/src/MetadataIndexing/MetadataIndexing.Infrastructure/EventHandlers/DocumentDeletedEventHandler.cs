using MetadataIndexing.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Shared.Events;

namespace MetadataIndexing.Infrastructure.EventHandlers;

public class DocumentDeletedEventHandler : IEventHandler<DocumentDeletedEvent>
{
    private readonly IMetadataIndexingUnitOfWork _unitOfWork;
    private readonly ILogger<DocumentDeletedEventHandler> _logger;

    public DocumentDeletedEventHandler(
        IMetadataIndexingUnitOfWork unitOfWork,
        ILogger<DocumentDeletedEventHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task HandleAsync(DocumentDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Marking document {DocumentId} as deleted in search index", @event.DocumentId);

            var searchIndex = await _unitOfWork.SearchDocumentIndexes.GetByDocumentIdAsync(@event.DocumentId, cancellationToken);
            if (searchIndex == null)
            {
                _logger.LogWarning("Document {DocumentId} is not indexed, skipping delete", @event.DocumentId);
                return;
            }

            searchIndex.MarkAsDeleted(@event.DeletedBy);
            _unitOfWork.SearchDocumentIndexes.Update(searchIndex);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully marked document {DocumentId} as deleted", @event.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark document {DocumentId} as deleted", @event.DocumentId);
            throw;
        }
    }
}
