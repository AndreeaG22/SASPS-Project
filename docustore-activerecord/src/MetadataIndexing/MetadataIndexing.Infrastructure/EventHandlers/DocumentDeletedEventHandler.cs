using MetadataIndexing.Domain.Entities;
using Microsoft.Extensions.Logging;
using Shared.Events;

namespace MetadataIndexing.Infrastructure.EventHandlers;

public class DocumentDeletedEventHandler : IEventHandler<DocumentDeletedEvent>
{
    private readonly ILogger<DocumentDeletedEventHandler> _logger;

    public DocumentDeletedEventHandler(ILogger<DocumentDeletedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(DocumentDeletedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Marking document {DocumentId} as deleted in search index", @event.DocumentId);

            var searchIndex = await SearchDocumentIndex.FindByDocumentId(@event.DocumentId, cancellationToken);
            if (searchIndex == null)
            {
                _logger.LogWarning("Document {DocumentId} is not indexed, skipping delete", @event.DocumentId);
                return;
            }

            searchIndex.MarkAsDeleted(@event.DeletedBy);
            await searchIndex.Save(cancellationToken);

            _logger.LogInformation("Successfully marked document {DocumentId} as deleted", @event.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark document {DocumentId} as deleted", @event.DocumentId);
            throw;
        }
    }
}
