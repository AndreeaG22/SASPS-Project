using Versioning.Domain.Entities;

namespace Document.Application.Interfaces;

/// <summary>
/// Read-only interface for version queries from Document module.
/// This allows Document.Application to query versions without creating a circular dependency.
/// </summary>
public interface IVersionQueryService
{
    Task<VersionEntity?> GetCurrentVersionAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task<byte[]> GetVersionFileAsync(Guid documentId, int versionNumber, CancellationToken cancellationToken = default);
}
