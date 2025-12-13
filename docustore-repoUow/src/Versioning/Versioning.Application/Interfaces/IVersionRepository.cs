using Versioning.Domain.Entities;

namespace Versioning.Application.Interfaces;

/// <summary>
/// Repository interface for Version entity operations.
/// </summary>
public interface IVersionRepository
{
    Task<VersionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VersionEntity?> GetByVersionNumberAsync(Guid documentId, int versionNumber, CancellationToken cancellationToken = default);
    Task<VersionEntity?> GetCurrentVersionAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<List<VersionEntity>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<List<VersionEntity>> GetCurrentVersionsForDocumentAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<int> GetNextVersionNumberAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<int> CountByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task AddAsync(VersionEntity version, CancellationToken cancellationToken = default);
    void Update(VersionEntity version);
    void UpdateRange(IEnumerable<VersionEntity> versions);
}
