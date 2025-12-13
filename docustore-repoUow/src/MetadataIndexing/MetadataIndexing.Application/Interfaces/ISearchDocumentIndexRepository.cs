using MetadataIndexing.Domain.Entities;

namespace MetadataIndexing.Application.Interfaces;

/// <summary>
/// Repository interface for SearchDocumentIndex entity operations.
/// </summary>
public interface ISearchDocumentIndexRepository
{
    Task<SearchDocumentIndex?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<SearchDocumentIndex?> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<List<SearchDocumentIndex>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(SearchDocumentIndex index, CancellationToken cancellationToken = default);
    void Update(SearchDocumentIndex index);
}
