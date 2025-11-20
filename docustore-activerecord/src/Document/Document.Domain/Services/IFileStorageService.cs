namespace Document.Domain.Services;

public interface IFileStorageService
{
    Task<string> CreateDocumentFolderAsync(Guid documentId, string fileName, CancellationToken cancellationToken = default);
    Task<string> SaveVersionFileAsync(Guid documentId, byte[] fileContent, int versionNumber, CancellationToken cancellationToken = default);
    Task<byte[]> GetVersionFileAsync(Guid documentId, int versionNumber, CancellationToken cancellationToken = default);
    Task<byte[]> GetCurrentVersionFileAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task DeleteDocumentFolderAsync(Guid documentId, CancellationToken cancellationToken = default);
    Task UpdateCurrentVersionMarkerAsync(Guid documentId, int versionNumber, CancellationToken cancellationToken = default);
    Task<int?> GetCurrentVersionNumberAsync(Guid documentId, CancellationToken cancellationToken = default);
}