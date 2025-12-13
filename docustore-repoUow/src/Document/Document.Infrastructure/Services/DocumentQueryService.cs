using Document.Application.Interfaces;
using Document.Domain.Enums;
using Versioning.Application.Interfaces;

namespace Document.Infrastructure.Services;

/// <summary>
/// Implementation of IDocumentQueryService that bridges Versioning and Document modules.
/// </summary>
public class DocumentQueryService : IDocumentQueryService
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentQueryService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<bool> IsDocumentActiveAsync(Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        return document != null && document.Status == DocumentStatus.Active;
    }
}
