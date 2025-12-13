using Document.Domain.Entities;

namespace Document.Application.Interfaces;

/// <summary>
/// Repository interface for Document entity operations.
/// Defines persistence operations abstracted from the domain.
/// </summary>
public interface IDocumentRepository
{
    Task<DocumentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<DocumentEntity>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<List<DocumentEntity>> GetAllIncludingDeletedAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(DocumentEntity document, CancellationToken cancellationToken = default);
    void Update(DocumentEntity document);
    void Remove(DocumentEntity document);
}
