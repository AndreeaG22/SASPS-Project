using Tagging.Domain.Entities;

namespace Tagging.Application.Interfaces;

/// <summary>
/// Repository interface for DocumentTag entity operations.
/// </summary>
public interface IDocumentTagRepository
{
    Task<DocumentTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DocumentTag?> GetByDocumentAndTagAsync(Guid documentId, Guid tagId, CancellationToken cancellationToken = default);
    Task<List<DocumentTag>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<List<DocumentTag>> GetByTagIdAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<List<DocumentTag>> GetByTagIdsAsync(List<Guid> tagIds, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid documentId, Guid tagId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetDocumentCountsByTagAsync(CancellationToken cancellationToken = default);
    Task AddAsync(DocumentTag documentTag, CancellationToken cancellationToken = default);
    void Remove(DocumentTag documentTag);
}
