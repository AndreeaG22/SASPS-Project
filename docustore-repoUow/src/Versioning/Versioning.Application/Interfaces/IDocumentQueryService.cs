namespace Versioning.Application.Interfaces;

/// <summary>
/// Read-only interface for document queries from Versioning module.
/// This allows Versioning.Application to query document status without creating a circular dependency.
/// </summary>
public interface IDocumentQueryService
{
    Task<bool> IsDocumentActiveAsync(Guid documentId, CancellationToken cancellationToken = default);
}
