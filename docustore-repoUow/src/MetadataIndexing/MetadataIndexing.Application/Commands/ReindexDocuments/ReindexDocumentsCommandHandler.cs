using Document.Application.Queries.ListDocuments;
using MediatR;
using MetadataIndexing.Application.Interfaces;
using MetadataIndexing.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace MetadataIndexing.Application.Commands.ReindexDocuments;

public class ReindexDocumentsCommandHandler : IRequestHandler<ReindexDocumentsCommand, int>
{
    private readonly IMediator _mediator;
    private readonly IMetadataIndexingUnitOfWork _unitOfWork;
    private readonly ILogger<ReindexDocumentsCommandHandler> _logger;

    public ReindexDocumentsCommandHandler(
        IMediator mediator,
        IMetadataIndexingUnitOfWork unitOfWork,
        ILogger<ReindexDocumentsCommandHandler> logger)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> Handle(ReindexDocumentsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting re-indexing of all documents");

            // Get all documents from Document module
            var listDocumentsQuery = new ListDocumentsQuery();
            var result = await _mediator.Send(listDocumentsQuery, cancellationToken);

            var indexedCount = 0;

            foreach (var doc in result.Documents)
            {
                try
                {
                    // Check if already indexed
                    var existingIndex = await _unitOfWork.SearchDocumentIndexes.GetByDocumentIdAsync(doc.Id, cancellationToken);
                    
                    if (existingIndex != null)
                    {
                        _logger.LogDebug("Document {DocumentId} already indexed, updating", doc.Id);
                        
                        // Update existing index
                        existingIndex.UpdateMetadata(
                            title: doc.Title,
                            description: doc.Description,
                            updatedBy: "system" // Reindex operation
                        );
                        
                        _unitOfWork.SearchDocumentIndexes.Update(existingIndex);
                    }
                    else
                    {
                        _logger.LogDebug("Indexing new document {DocumentId}", doc.Id);
                        
                        // Create new index entry
                        var searchIndex = SearchDocumentIndex.Create(
                            documentId: doc.Id,
                            title: doc.Title,
                            description: doc.Description,
                            fileName: doc.FileName,
                            contentType: doc.ContentType,
                            fileSizeInBytes: doc.FileSizeInBytes,
                            createdBy: doc.CreatedBy,
                            createdAt: doc.CreatedAt
                        );

                        await _unitOfWork.SearchDocumentIndexes.AddAsync(searchIndex, cancellationToken);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    indexedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to index document {DocumentId}", doc.Id);
                    // Continue with other documents
                }
            }

            _logger.LogInformation("Re-indexing completed. Indexed {Count} documents", indexedCount);
            return indexedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to re-index documents");
            throw;
        }
    }
}
