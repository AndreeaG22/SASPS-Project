namespace Versioning.Domain.Services;

public interface IFileStorageService
{
    Task<string> SaveVersionFileAsync(Guid documentId, byte[] fileContent, int versionNumber, CancellationToken cancellationToken = default);
    Task<byte[]> GetVersionFileAsync(Guid documentId, int versionNumber, CancellationToken cancellationToken = default);
    Task UpdateCurrentVersionMarkerAsync(Guid documentId, int versionNumber, CancellationToken cancellationToken = default);
}